
using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Enums;
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_Parking_Reservation_System.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;

        public BookingRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Booking> CreateWithHoldAsync(Booking booking, IEnumerable<int> seatIds, int? parkingSlotId)
        {
            var seatIdList = seatIds.ToList();

            await using var tx = await _db.Database.BeginTransactionAsync();

            // Re-check every seat inside the transaction with a row lock, so two customers
            // racing for the same seat cannot both succeed (BRD Business Rule #1, "transaction check").
            foreach (var seatId in seatIdList)
            {
                var isTaken = await _db.BookingSeats
                    .FromSqlInterpolated($@"
                        SELECT bs.* FROM BookingSeats bs WITH (UPDLOCK, ROWLOCK)
                        INNER JOIN Bookings b ON b.BookingId = bs.BookingId
                        WHERE bs.SeatId = {seatId} AND b.Status IN ({BookingStatus.Pending.ToString()}, {BookingStatus.Confirmed.ToString()})")
                    .AnyAsync();

                if (isTaken)
                {
                    throw new SeatAlreadyBookedException(seatId);
                }
            }

            if (parkingSlotId.HasValue)
            {
                var slotTaken = await _db.Bookings
                    .FromSqlInterpolated($@"
                        SELECT * FROM Bookings WITH (UPDLOCK, ROWLOCK)
                        WHERE ParkingSlotId = {parkingSlotId.Value}
                        AND Status IN ({BookingStatus.Pending.ToString()}, {BookingStatus.Confirmed.ToString()})")
                    .AnyAsync();

                if (slotTaken)
                {
                    throw new ParkingSlotAlreadyReservedException(parkingSlotId.Value);
                }
            }

            booking.BookingSeats = seatIdList.Select(id => new BookingSeat { SeatId = id }).ToList();
            _db.Bookings.Add(booking);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return booking;
        }

        public Task<Booking?> GetByIdAsync(int bookingId) =>
            _db.Bookings.Include(b => b.BookingSeats).FirstOrDefaultAsync(b => b.BookingId == bookingId);

        public Task<IReadOnlyList<Booking>> GetByCustomerIdAsync(int customerId) =>
            QueryAsList(_db.Bookings.Include(b => b.BookingSeats)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.CreatedAt));

        public Task<IReadOnlyList<Booking>> GetByEventIdAsync(int eventId) =>
            QueryAsList(_db.Bookings.Include(b => b.BookingSeats)
                .Where(b => b.EventId == eventId)
                .OrderByDescending(b => b.CreatedAt));

        public async Task CancelAsync(int bookingId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            booking.Status = BookingStatus.Cancelled;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = DateTime.UtcNow;
            // Seats/parking release back to Available is performed by the Seat/Parking
            // modules, triggered off this status change (see BookingService for the
            // cross-module release call), keeping ownership of those tables in their
            // own modules per BRD 7.4 layered architecture.

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task<int> ExpireHeldBookingsAsync(DateTime now)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var expired = await _db.Bookings
                .Where(b => b.Status == BookingStatus.Pending && b.HoldExpiresAt != null && b.HoldExpiresAt < now)
                .ToListAsync();

            foreach (var booking in expired)
            {
                booking.Status = BookingStatus.Expired;
                booking.HoldExpiresAt = null;
                booking.UpdatedAt = now;
            }

            var count = await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return expired.Count;
        }

        public async Task ConfirmAsync(int bookingId)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            booking.Status = BookingStatus.Confirmed;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public Task<IReadOnlyList<Booking>> GetPendingWithExpiredHoldsAsync(DateTime now) =>
            QueryAsList(_db.Bookings.Where(b =>
                b.Status == BookingStatus.Pending && b.HoldExpiresAt != null && b.HoldExpiresAt < now));

        private static async Task<IReadOnlyList<Booking>> QueryAsList(IQueryable<Booking> query) =>
            await query.ToListAsync();
    }
}

    

