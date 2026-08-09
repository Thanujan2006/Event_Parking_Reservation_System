
using Microsoft.EntityFrameworkCore;
using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.DTOs;
using Event_Parking_Reservation_System.Enums;
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Models;

namespace Event_Parking_Reservation_System.Services
{
    /// <summary>
    /// Implements BRD Section 4.5 (Parking Reservation) end-to-end:
    ///   - Admin layout management (create/edit/remove slots + fee)
    ///   - Customer optional single-slot reservation, race-safe
    ///   - Immutable fee snapshot at reservation time
    /// All rule numbers referenced in comments map to BRD 4.5.5 Business Rules table.
    /// </summary>
    public class ParkingService : IParkingService
    {
        private readonly AppDbContext _db;

        public ParkingService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ParkingSlotDto>> GetLayoutAsync(int eventId)
        {
            var slots = await _db.ParkingSlots
                .Where(s => s.EventId == eventId)
                .OrderBy(s => s.SlotNumber)
                .ToListAsync();

            if (!slots.Any())
            {
                // BRD 4.5.11: GET returns 404 when the event has no layout yet.
                throw new NotFoundException("PARKING_LAYOUT_NOT_FOUND",
                    "No parking layout has been configured for this event.");
            }

            return slots.Select(ToDto).ToList();
        }

        public async Task<List<ParkingSlotDto>> CreateLayoutAsync(int eventId, ParkingLayoutCreateDto dto)
        {
            // Rule #3: only Admin may create/modify layout & fee — enforced by [Authorize(Roles="Admin")]
            // on the controller action, not repeated here.
            if (dto.Slots == null || dto.Slots.Count == 0)
            {
                throw new BadRequestException("EMPTY_SLOT_LIST", "At least one parking slot is required.");
            }

            // Reject duplicate SlotNumbers within the same request payload before touching the DB.
            var duplicateInPayload = dto.Slots
                .GroupBy(s => s.SlotNumber.Trim(), StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1);
            if (duplicateInPayload) ;
            {
                throw new BadRequestException("DUPLICATE_SLOT_NUMBER",
                    "Slot numbers must be unique within the submitted layout.");
            }

            var newSlots = dto.Slots.Select(s => new ParkingSlot
            {
                EventId = eventId,
                SlotNumber = s.SlotNumber.Trim(),
                Fee = s.Fee,
                Status = ParkingSlotStatus.Available
            }).ToList();

            _db.ParkingSlots.AddRange(newSlots);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Existing DB rows already use one of these SlotNumbers for this event.
                throw new BadRequestException("DUPLICATE_SLOT_NUMBER",
                    "One or more slot numbers already exist for this event.");
            }

            return newSlots.Select(ToDto).ToList();
        }

        public async Task<ParkingSlotDto> UpdateSlotAsync(int eventId, int slotId, ParkingSlotUpdateDto dto)
        {
            var slot = await _db.ParkingSlots
                .Include(s => s.Reservation)
                .FirstOrDefaultAsync(s => s.SlotId == slotId && s.EventId == eventId);

            if (slot == null)
            {
                throw new NotFoundException("SLOT_NOT_FOUND", "Parking slot not found for this event.");
            }

            // Rule #5 (immutability): editing the Fee must NEVER retroactively change a fee
            // that has already been snapshotted onto an existing reservation. If a reservation
            // exists we still allow renumbering (cosmetic) but block Fee changes outright to make
            // the immutability guarantee unambiguous rather than silently ignoring the field.
            if (slot.Reservation != null && dto.Fee.HasValue && dto.Fee.Value != slot.Fee)
            {
                throw new ConflictException("ACTIVE_RESERVATION_FEE_LOCKED",
                    "Cannot change the fee for a slot that has an active reservation.");
            }

            if (dto.SlotNumber != null)
            {
                slot.SlotNumber = dto.SlotNumber.Trim();
            }
            if (dto.Fee.HasValue && slot.Reservation == null)
            {
                slot.Fee = dto.Fee.Value;
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new BadRequestException("DUPLICATE_SLOT_NUMBER",
                    "This slot number already exists for this event.");
            }

            return ToDto(slot);
        }

        public async Task RemoveSlotAsync(int eventId, int slotId)
        {
            var slot = await _db.ParkingSlots
                .Include(s => s.Reservation)
                .FirstOrDefaultAsync(s => s.SlotId == slotId && s.EventId == eventId);

            if (slot == null)
            {
                throw new NotFoundException("SLOT_NOT_FOUND", "Parking slot not found for this event.");
            }

            // Rule #4: cannot delete a slot with an active reservation.
            if (slot.Reservation != null)
            {
                throw new ConflictException("ACTIVE_RESERVATION_EXISTS",
                    "Cannot remove a slot with an active reservation.");
            }

            _db.ParkingSlots.Remove(slot);
            await _db.SaveChangesAsync();
        }

        public async Task<ParkingReservationDto> ReserveAsync(int bookingId, ParkingReserveDto dto)
        {
            // Whole operation is one atomic transaction (BRD 4.5.9 step 6: "Transaction — Slot
            // Status Re-check + Lock"). Serializable isolation + the unique index on
            // ParkingReservations.SlotId together guarantee that under true concurrency, exactly
            // one of two simultaneous requests for the same slot wins — this satisfies AC2.
            using var transaction = await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            try
            {
                // Rule: a booking may hold at most one parking slot (unique index on BookingId
                // also enforces this at the DB layer as a backstop).
                var existingForBooking = await _db.ParkingReservations
                    .AnyAsync(r => r.BookingId == bookingId);
                if (existingForBooking)
                {
                    throw new ConflictException("BOOKING_ALREADY_HAS_PARKING",
                        "This booking already has a parking reservation.");
                }

                var slot = await _db.ParkingSlots.FirstOrDefaultAsync(s => s.SlotId == dto.SlotId);
                if (slot == null)
                {
                    throw new NotFoundException("SLOT_NOT_FOUND", "Parking slot not found.");
                }

                if (slot.Status != ParkingSlotStatus.Available)
                {
                    // Covers both "already Reserved" and "Held by another in-flight request".
                    throw new ConflictException("SLOT_ALREADY_RESERVED",
                        "Slot already reserved for this event.");
                }

                // Rule #5: fee is fixed (snapshotted) at the moment of reservation.
                var reservation = new ParkingReservation
                {
                    BookingId = bookingId,
                    SlotId = slot.SlotId,
                    FeeAtReservation = slot.Fee,
                    CreatedAt = DateTime.UtcNow
                };

                slot.Status = ParkingSlotStatus.Reserved;

                _db.ParkingReservations.Add(reservation);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ParkingReservationDto
                {
                    ReservationId = reservation.ReservationId,
                    BookingId = reservation.BookingId,
                    SlotId = reservation.SlotId,
                    SlotNumber = slot.SlotNumber,
                    FeeAtReservation = reservation.FeeAtReservation,
                    CreatedAt = reservation.CreatedAt
                };
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Losing side of a genuine race: the other request committed first and the
                // unique index on SlotId (or BookingId) rejected this insert.
                await transaction.RollbackAsync();
                throw new ConflictException("SLOT_ALREADY_RESERVED",
                    "This parking slot was just reserved by another customer.");
            }
            catch (DomainException)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RemoveReservationAsync(int bookingId)
        {
            // BRD 4.5.11: DELETE /api/bookings/{bookingId}/parking — only valid before the
            // booking is finalized (Confirmed). Finalization state itself belongs to Module 6
            // (Booking Management); this service only guards against a missing reservation.
            var reservation = await _db.ParkingReservations
                .Include(r => r.ParkingSlot)
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);

            if (reservation == null)
            {
                throw new NotFoundException("RESERVATION_NOT_FOUND",
                    "No parking reservation found for this booking.");
            }

            if (reservation.ParkingSlot != null)
            {
                reservation.ParkingSlot.Status = ParkingSlotStatus.Available;
            }

            _db.ParkingReservations.Remove(reservation);
            await _db.SaveChangesAsync();
        }

        private static ParkingSlotDto ToDto(ParkingSlot slot) => new()
        {
            SlotId = slot.SlotId,
            EventId = slot.EventId,
            SlotNumber = slot.SlotNumber,
            Status = slot.Status.ToString(),
            Fee = slot.Fee
        };

        /// <summary>
        /// Detects a unique-constraint violation regardless of provider (SQL Server error 2601/2627,
        /// or the generic message fallback for providers used in tests, e.g. Sqlite/InMemory).
        /// </summary>
        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("2601") || message.Contains("2627") ||
                   message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
        }
    }
}
