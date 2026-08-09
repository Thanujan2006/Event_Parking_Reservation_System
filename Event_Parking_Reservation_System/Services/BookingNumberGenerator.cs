using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Event_Parking_Reservation_System.Services
{
    public class BookingNumberGenerator
    {
        private readonly BookingDbContext _db;
        private readonly IDateTimeProvider _clock;

        public BookingNumberGenerator(BookingDbContext db, IDateTimeProvider clock)
        {
            _db = db;
            _clock = clock;
        }

        public async Task<string> GenerateAsync()
        {
            var year = _clock.UtcNow.Year;

            // SQL Server row lock: two concurrent callers serialize on this row instead of
            // both reading the same LastNumber and colliding.
            await using var tx = await _db.Database.BeginTransactionAsync();

            var sequence = await _db.BookingSequences
                .FromSqlInterpolated($"SELECT * FROM BookingSequences WITH (UPDLOCK, ROWLOCK) WHERE Year = {year}")
                .FirstOrDefaultAsync();

            if (sequence is null)
            {
                sequence = new Entities.BookingSequence { Year = year, LastNumber = 0 };
                _db.BookingSequences.Add(sequence);
            }

            sequence.LastNumber += 1;
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return $"BKG-{year}-{sequence.LastNumber:D6}";
        }
    }
}
