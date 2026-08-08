
using Event_Parking_Reservation_System.DTOs;
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Models;
using Event_Parking_Reservation_System.Repositories;




namespace Event_Parking_Reservation_System.Services                              
{
    /// <summary>s
    /// Encodes Module 4 business rules (BRD 4.4.5–4.4.6):
    /// - Seat map row*column count must equal event capacity (AC2)s
    /// - Seat holds are race-safe: concurrent selection of the same seat
    ///   yields exactly one winner, others get "already booked" (AC1)
    /// - Seats with an active (Held/Booked) booking cannot be deleted (AC3)
    /// </summary>
    public class SeatService : ISeatServices
    {
        private readonly SeatRepository _repository;

        public SeatService(SeatRepository repository)
        {
            _repository = repository;
        }

        public async Task GenerateSeatMapAsync(int eventId, GenerateSeatMapRequest request)
        {
            if (!await _repository.EventExistsAsync(eventId))
            {
                throw new EventNotFoundException(eventId);
            }

            if (await _repository.GetSeatCountForEventAsync(eventId) > 0)
            {
                throw new SeatMapAlreadyExistsException(eventId);
            }

            // AC2: Rows x Columns must equal event capacity, or the seat map
            // is rejected outright and nothing is saved.
            var seatMapCount = request.Rows * request.Columns;
            var eventCapacity = await _repository.GetEventCapacityAsync(eventId) ?? 0;

            if (seatMapCount != eventCapacity)
            {
                throw new SeatCountMismatchException(seatMapCount, eventCapacity);
            }

            var seats = new List<Seat>(seatMapCount);
            for (var row = 0; row < request.Rows; row++)
            {
                var rowLabel = ToRowLabel(row);
                for (var col = 1; col <= request.Columns; col++)
                {
                    seats.Add(new Seat
                    {
                        EventId = eventId,
                        RowLabel = rowLabel,
                        ColumnNumber = col,
                        SeatNumber = $"{rowLabel}{col}", // unique within event (BRD 4.4.12)
                        SeatType = request.SeatType,
                        PriceOverride = request.PriceOverride,
                        Status = SeatStatus.Available,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _repository.AddRangeAsync(seats);
        }

        public async Task<SeatMapDto> GetSeatMapAsync(int eventId)
        {
            if (!await _repository.EventExistsAsync(eventId))
            {
                throw new EventNotFoundException(eventId);
            }

            var seats = await _repository.GetByEventIdAsync(eventId);
            var basePrice = await _repository.GetEventBasePriceAsync(eventId);

            var rows = seats.Select(s => s.RowLabel).Distinct().Count();
            var columns = seats.Count == 0 ? 0 : seats.Max(s => s.ColumnNumber);

            return new SeatMapDto
            {
                EventId = eventId,
                Rows = rows,
                Columns = columns,
                Seats = seats.Select(s => MapToDto(s, basePrice)).ToList()
            };
        }

        public async Task<HoldSeatsResult> HoldSeatsAsync(int bookingId, HoldSeatsRequest request)
        {
            var heldSeatIds = await _repository.TryHoldSeatsAsync(request.SeatIds, bookingId);

            if (heldSeatIds.Count != request.SeatIds.Count)
            {
                // At least one seat lost the race — roll back everything this
                // call held so we don't leave a partial hold, then surface
                // exactly which seats were unavailable (AC1: 409 "Seat already booked").
                await _repository.ReleaseHeldSeatsAsync(bookingId);

                var unavailable = request.SeatIds.Except(heldSeatIds).ToList();
                throw new SeatsUnavailableException(unavailable);
            }

            decimal runningTotal = 0;
            foreach (var seatId in heldSeatIds)
            {
                var seat = await _repository.GetByIdAsync(seatId);
                if (seat is not null)
                {
                    var basePrice = await _repository.GetEventBasePriceAsync(seat.EventId);
                    runningTotal += seat.PriceOverride ?? basePrice;
                }
            }

            return new HoldSeatsResult
            {
                Success = true,
                HeldSeatIds = heldSeatIds,
                UnavailableSeatIds = Array.Empty<int>(),
                RunningTotal = runningTotal
            };
        }

        public async Task ConfirmSeatsAsync(int bookingId)
        {
            await _repository.ConfirmSeatsAsync(bookingId);
        }

        public async Task ReleaseHeldSeatsAsync(int bookingId)
        {
            await _repository.ReleaseHeldSeatsAsync(bookingId);
        }

        public async Task DeleteSeatAsync(int seatId)
        {
            var seat = await _repository.GetByIdAsync(seatId)
                ?? throw new SeatNotFoundException(seatId);

            // AC3: block delete/edit while the seat has an active booking.
            if (seat.Status != SeatStatus.Available)
            {
                throw new SeatHasActiveBookingException(seatId);
            }

            await _repository.DeleteAsync(seatId);
        }

        /// <summary>Converts a zero-based row index to a spreadsheet-style label: 0->A, 1->B, ... 25->Z, 26->AA.</summary>
        private static string ToRowLabel(int index)
        {
            var label = string.Empty;
            index++;
            while (index > 0)
            {
                var rem = (index - 1) % 26;
                label = (char)('A' + rem) + label;
                index = (index - 1) / 26;
            }
            return label;
        }

        private static DTOs.SeatDto MapToDto(Seat s, decimal basePrice) => new()
        {
            SeatId = s.SeatId,
            RowLabel = s.RowLabel,
            ColumnNumber = s.ColumnNumber,
            SeatNumber = s.SeatNumber,
            SeatType = s.SeatType,
            EffectivePrice = s.PriceOverride ?? basePrice,
            Status = s.Status.ToString()
        };
    }
}
