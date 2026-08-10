using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Models;
using Event_Parking_Reservation_System.Services;
using Microsoft.EntityFrameworkCore;

namespace Event_Parking_Reservation_System.Repositories
{
   public class VenueCategoryRepository : IVenueCategoryRepository
    {
        public class VenueRepository : IVenueRepository
        {
            private readonly AppDbContext _db;

            public VenueRepository(AppDbContext db)
            {
                _db = db;
            }

            public async Task<Venue?> GetByIdAsync(int venueId)
            {
                return await _db.Set<Venue>().AsNoTracking()
                    .FirstOrDefaultAsync(v => v.VenueId == venueId);
            }

            public async Task<IReadOnlyList<Venue>> GetAllAsync()
            {
                return await _db.Set<Venue>().AsNoTracking()
                    .OrderBy(v => v.Name)
                    .ToListAsync();
            }

            public async Task<int> AddAsync(Venue venue)
            {
                await _db.Set<Venue>().AddAsync(venue);
                await _db.SaveChangesAsync();
                return venue.VenueId;
            }

            public async Task UpdateAsync(Venue venue)
            {
                _db.Set<Venue>().Update(venue);
                await _db.SaveChangesAsync();
            }

            public async Task DeleteAsync(int venueId)
            {
                var venue = await _db.Set<Venue>().FindAsync(venueId);
                if (venue is not null)
                {
                    _db.Set<Venue>().Remove(venue);
                    await _db.SaveChangesAsync();
                }
            }

            public async Task<bool> HasUpcomingEventsAsync(int venueId)
            {
                // Event entity lives in Module 3 (Event Management). Queried via
                // raw SQL against the shared schema to avoid a hard compile-time
                // dependency between modules.
                var count = await _db.Database
                    .SqlQuery<int>($@"
                    SELECT COUNT(*) AS Value
                    FROM Events
                    WHERE VenueId = {venueId}
                      AND EventDateTime >= GETUTCDATE()")
                    .FirstOrDefaultAsync();

                return count > 0;
            }

            public async Task<IReadOnlyList<Venue>> GetAvailableAsync(DateTime startTime, DateTime endTime)
            {
                // A venue is available if no existing event's [EventDateTime, EventDateTime + Duration)
                // overlaps the requested window. Overlap check is scoped to the same venue only (BRD 4.3.4).
                return await _db.Set<Venue>()

                    .FromSqlInterpolated($@"
                    SELECT v.* FROM Venues v
                    WHERE NOT EXISTS (
                        SELECT 1 FROM Events e
                        WHERE e.VenueId = v.VenueId
                          AND e.EventDateTime < {endTime}
                          AND DATEADD(MINUTE, e.DurationMinutes, e.EventDateTime) > {startTime}
                    )")
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public class EventCategoryRepository : IEventCategoryRepository
        {
            private readonly AppDbContext _db;

            public EventCategoryRepository(AppDbContext db)
            {
                _db = db;
            }

            public async Task<EventCategory?> GetByIdAsync(int categoryId)
            {
                return await _db.Set<EventCategory>().AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            }

            public async Task<IReadOnlyList<EventCategory>> GetAllAsync()
            {
                return await _db.Set<EventCategory>().AsNoTracking()
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }

            public async Task<bool> NameExistsAsync(string name, int? excludeCategoryId = null)
            {
                var query = _db.Set<EventCategory>().Where(c => c.Name.ToLower() == name.ToLower());

                if (excludeCategoryId.HasValue)
                {
                    query = query.Where(c => c.CategoryId != excludeCategoryId.Value);
                }

                return await query.AnyAsync();
            }

            public async Task<int> AddAsync(EventCategory category)
            {
                await _db.Set<EventCategory>().AddAsync(category);
                await _db.SaveChangesAsync();
                return category.CategoryId;
            }

            public async Task UpdateAsync(EventCategory category)
            {
                _db.Set<EventCategory>().Update(category);
                await _db.SaveChangesAsync();
            }

            public async Task DeleteAsync(int categoryId)
            {
                var category = await _db.Set<EventCategory>().FindAsync(categoryId);
                if (category is not null)
                {
                    _db.Set<EventCategory>().Remove(category);
                    await _db.SaveChangesAsync();
                }
            }

            public async Task<bool> IsInUseAsync(int categoryId)
            {
                var count = await _db.Database
                    .SqlQuery<int>($@"
                    SELECT COUNT(*) AS Value
                    FROM Events
                    WHERE CategoryId = {categoryId}")
                    .FirstOrDefaultAsync();

                return count > 0;
            }
        }
    }
}
   