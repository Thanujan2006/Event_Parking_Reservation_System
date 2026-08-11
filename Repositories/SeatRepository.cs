using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Event_Parking_Reservation_System.Repositories
{
    public class SeatRepository : ISeatRepository
    {
        private readonly AppDbContext _db;

        public SeatRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Seat?> GetByIdAsync(int seatId)
        {
            return await _db.Set<Seat>().AsNoTracking()
                .FirstOrDefaultAsync(s => s.SeatId == seatId);
        }

        public async Task<IReadOnlyList<Seat>> GetByEventIdAsync(int eventId)
        {
            return await _db.Set<Seat>().AsNoTracking()
                .Where(s => s.EventId == eventId)
                .OrderBy(s => s.RowLabel).ThenBy(s => s.ColumnNumber)
                .ToListAsync();
        }

        public async Task<int> GetSeatCountForEventAsync(int eventId)
        {
            return await _db.Set<Seat>().CountAsync(s => s.EventId == eventId);
        }

        public async Task<int> CountAvailableSeatsForEventAsync(int eventId)
        {
            return await _db.Set<Seat>().CountAsync(s =>
                s.EventId == eventId && s.Status == SeatStatus.Available);
        }

        public async Task AddRangeAsync(IEnumerable<Seat> seats)
        {
            await _db.Set<Seat>().AddRangeAsync(seats);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int seatId)
        {
            var seat = await _db.Set<Seat>().FindAsync(seatId);
            if (seat is not null)
            {
                _db.Set<Seat>().Remove(seat);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<int>> TryHoldSeatsAsync(IReadOnlyList<int> seatIds, int bookingId)
        {
            var heldSeatIds = new List<int>();

            // Each seat is claimed with a single conditional UPDATE
            // (WHERE Status = Available), so the database itself serializes
            // concurrent attempts on the same row — the second caller's UPDATE
            // simply affects 0 rows instead of racing in application code.
            // Wrapped in one transaction so a partial hold never lingers on failure.
            await using IDbContextTransaction tx = await _db.Database.BeginTransactionAsync();

            foreach (var seatId in seatIds)
            {
                var rowsAffected = await _db.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE Seats
                    SET Status = 1, BookingId = {bookingId}
                    WHERE SeatId = {seatId} AND Status = 0");

                if (rowsAffected > 0)
                {
                    heldSeatIds.Add(seatId);
                }
            }

            await tx.CommitAsync();

            return heldSeatIds;
        }

        public async Task ConfirmSeatsAsync(int bookingId)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Seats SET Status = 2 WHERE BookingId = {bookingId} AND Status = 1");
        }

        public async Task ReleaseHeldSeatsAsync(int bookingId)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Seats SET Status = 0, BookingId = NULL WHERE BookingId = {bookingId} AND Status = 1");
        }

        public async Task<int?> GetEventCapacityAsync(int eventId)
        {
            // Event entity lives in Module 3; queried via raw SQL to avoid a
            // hard compile-time dependency between modules.
            return await _db.Database.SqlQuery<int>($@"
                    SELECT Capacity AS Value FROM Events WHERE EventId = {eventId}")
                .FirstOrDefaultAsync();
        }

        public async Task<bool> EventExistsAsync(int eventId)
        {
            var count = await _db.Database.SqlQuery<int>($@"
                    SELECT COUNT(*) AS Value FROM Events WHERE EventId = {eventId}")
                .FirstOrDefaultAsync();
            return count > 0;
        }

        public async Task<decimal> GetEventBasePriceAsync(int eventId)
        {
            return await _db.Database.SqlQuery<decimal>($@"
                    SELECT TicketPrice AS Value FROM Events WHERE EventId = {eventId}")
                .FirstOrDefaultAsync();
        }


    }
}
