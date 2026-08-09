using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_Parking_Reservation_System.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly DbContext _db;

        public EventRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<Event?> GetByIdAsync(int eventId)
        {
            return await _db.Set<Event>().AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == eventId);
        }

        public async Task<(IReadOnlyList<Event> Items, int TotalCount)> SearchAsync(
            string? name, DateTime? date, int? venueId, int? categoryId, int page, int pageSize)
        {
            var query = _db.Set<Event>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var term = name.Trim().ToLower();
                query = query.Where(e => e.Name.ToLower().Contains(term));
            }

            if (date.HasValue)
            {
                var day = date.Value.Date;
                query = query.Where(e => e.EventDateTime >= day && e.EventDateTime < day.AddDays(1));
            }

            if (venueId.HasValue)
            {
                query = query.Where(e => e.VenueId == venueId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.EventDateTime)
                .Skip(Math.Max(0, page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> AddAsync(Event ev)
        {
            await _db.Set<Event>().AddAsync(ev);
            await _db.SaveChangesAsync();
            return ev.EventId;
        }

        public async Task UpdateAsync(Event ev)
        {
            _db.Set<Event>().Update(ev);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int eventId)
        {
            var ev = await _db.Set<Event>().FindAsync(eventId);
            if (ev is not null)
            {
                _db.Set<Event>().Remove(ev);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> HasOverlapAsync(
            int venueId, DateTime eventDateTime, int durationMinutes, int? excludeEventId = null)
        {
            var newEnd = eventDateTime.AddMinutes(durationMinutes);

            // Standard interval-overlap predicate: two ranges [aStart,aEnd) and
            // [bStart,bEnd) overlap iff aStart < bEnd AND bStart < aEnd.
            // Expressed as raw SQL because DATEADD-based interval arithmetic is
            // not translatable by the EF Core LINQ provider.
            var overlapCount = await _db.Database.SqlQuery<int>($@"
                    SELECT COUNT(*) AS Value
                    FROM Events
                    WHERE VenueId = {venueId}
                      AND EventId <> {excludeEventId ?? 0}
                      AND EventDateTime < {newEnd}
                      AND DATEADD(MINUTE, DurationMinutes, EventDateTime) > {eventDateTime}")
                .FirstOrDefaultAsync();

            return overlapCount > 0;
        }

        public async Task<int?> GetVenueCapacityAsync(int venueId)
        {
            var capacity = await _db.Database.SqlQuery<int>($@"
                    SELECT TotalCapacity AS Value
                    FROM Venues
                    WHERE VenueId = {venueId}")
                .FirstOrDefaultAsync();

            return capacity;
        }

        public async Task<bool> VenueExistsAsync(int venueId)
        {
            var count = await _db.Database.SqlQuery<int>($@"
                    SELECT COUNT(*) AS Value FROM Venues WHERE VenueId = {venueId}")
                .FirstOrDefaultAsync();
            return count > 0;
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            var count = await _db.Database.SqlQuery<int>($@"
                    SELECT COUNT(*) AS Value FROM EventCategories WHERE CategoryId = {categoryId}")
                .FirstOrDefaultAsync();
            return count > 0;
        }

        public async Task<string> GetVenueNameAsync(int venueId)
        {
            var name = await _db.Database.SqlQuery<string>($@"
                    SELECT Name AS Value FROM Venues WHERE VenueId = {venueId}")
                .FirstOrDefaultAsync();
            return name ?? string.Empty;
        }

        public async Task<string> GetCategoryNameAsync(int categoryId)
        {
            var name = await _db.Database.SqlQuery<string>($@"
                    SELECT Name AS Value FROM EventCategories WHERE CategoryId = {categoryId}")
                .FirstOrDefaultAsync();
            return name ?? string.Empty;
        }
    }
}