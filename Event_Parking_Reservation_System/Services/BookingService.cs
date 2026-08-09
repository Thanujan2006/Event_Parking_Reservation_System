using Event_Parking_Reservation_System.Configuration;
using Event_Parking_Reservation_System.Dtos.BookingDtos;
using Event_Parking_Reservation_System.Enums;
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using Microsoft.Extensions.Options;

namespace Event_Parking_Reservation_System.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repository;
        private readonly IBookingNumberGenerator _numberGenerator;
        private readonly IDateTimeProvider _clock;
        private readonly BookingSettings _settings;

        public BookingService(
            IBookingRepository repository,
            IBookingNumberGenerator numberGenerator,
            IDateTimeProvider clock,
            IOptions<BookingSettings> settings)
        {
            _repository = repository;
            _numberGenerator = numberGenerator;
            _clock = clock;
            _settings = settings.Value;
        }

        public async Task<CreateBookingResponse> CreateBookingAsync(
            CreateBookingRequest request)
        {
            // BRD Business Rule #1 / AC1:
            // Reject an empty seat list before touching the DB.
            if (request.SeatIds is null || request.SeatIds.Count == 0)
            {
                throw new EmptySeatListException();
            }

            var now = _clock.UtcNow;

            var bookingNumber =
                await _numberGenerator.GenerateAsync();

            var booking = new Booking
            {
                BookingNumber = bookingNumber,
                CustomerId = request.CustomerId,
                EventId = request.EventId,
                Status = BookingStatus.Pending,

                // HoldExpiresAt = CreatedAt + configurable hold period
                HoldExpiresAt =
                    now.AddMinutes(_settings.HoldPeriodMinutes),

                ParkingSlotId = request.ParkingSlotId,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created =
                await _repository.CreateWithHoldAsync(
                    booking,
                    request.SeatIds,
                    request.ParkingSlotId);

            return new CreateBookingResponse
            {
                BookingId = created.BookingId,
                BookingNumber = created.BookingNumber,
                Status = created.Status.ToString(),
                HoldExpiresAt = created.HoldExpiresAt
            };
        }

        public async Task<BookingDtos> GetByIdAsync(
            int bookingId,
            int requestingCustomerId,
            bool isAdmin)
        {
            var booking =
                await _repository.GetByIdAsync(bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            if (!isAdmin &&
                booking.CustomerId != requestingCustomerId)
            {
                throw new BookingForbiddenException();
            }

            return ToDto(booking);
        }

        public async Task<IReadOnlyList<BookingDtos>>
            GetCustomerHistoryAsync(int customerId)
        {
            var bookings =
                await _repository.GetByCustomerIdAsync(customerId);

            return bookings.Select(ToDto).ToList();
        }

        public async Task<IReadOnlyList<BookingDtos>>
            GetByEventIdAsync(int eventId)
        {
            var bookings =
                await _repository.GetByEventIdAsync(eventId);

            return bookings.Select(ToDto).ToList();
        }

        public async Task<HoldStatusResponse>
            GetHoldStatusAsync(int bookingId)
        {
            var booking =
                await _repository.GetByIdAsync(bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            var remaining = 0;

            if (booking.Status == BookingStatus.Pending &&
                booking.HoldExpiresAt.HasValue)
            {
                var seconds =
                    (booking.HoldExpiresAt.Value - _clock.UtcNow)
                    .TotalSeconds;

                remaining =
                    seconds > 0
                        ? (int)Math.Ceiling(seconds)
                        : 0;
            }

            return new HoldStatusResponse
            {
                BookingId = booking.BookingId,
                Status = booking.Status.ToString(),
                RemainingSeconds = remaining
            };
        }

        public async Task CancelAsync(
            int bookingId,
            int requestingCustomerId,
            bool isAdmin)
        {
            var booking =
                await _repository.GetByIdAsync(bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            // User must own the booking unless admin
            if (!isAdmin &&
                booking.CustomerId != requestingCustomerId)
            {
                throw new BookingForbiddenException();
            }

            await _repository.CancelAsync(bookingId);
        }

        public async Task ConfirmAsync(int bookingId)
        {
            var booking =
                await _repository.GetByIdAsync(bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            if (booking.Status == BookingStatus.Expired)
            {
                throw new BookingExpiredException();
            }

            await _repository.ConfirmAsync(bookingId);
        }

        public async Task<int> ExpireOverdueHoldsAsync()
        {
            return await _repository
                .ExpireHeldBookingsAsync(_clock.UtcNow);
        }

        private static BookingDtos ToDto(Booking booking)
        {
            return new BookingDtos
            {
                BookingId = booking.BookingId,
                BookingNumber = booking.BookingNumber,
                CustomerId = booking.CustomerId,
                EventId = booking.EventId,
                Status = booking.Status.ToString(),
                HoldExpiresAt = booking.HoldExpiresAt,

                SeatIds = booking.BookingSeats
                    .Select(bs => bs.SeatId)
                    .ToList(),

                ParkingSlotId = booking.ParkingSlotId,
                CreatedAt = booking.CreatedAt
            };
        }
    }
}