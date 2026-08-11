using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event_Parking_Reservation_System.Models;

namespace Event_Parking_Reservation_System.Repositories
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(int eventId);

        Task<(IReadOnlyList<Event> Items, int TotalCount)> SearchAsync(
            string? name, DateTime? date, int? venueId, int? categoryId, int page, int pageSize);

        Task<int> AddAsync(Event ev);

        Task UpdateAsync(Event ev);

        Task DeleteAsync(int eventId);

        /// <summary>
        /// True if any other event at the same venue overlaps
        /// [eventDateTime, eventDateTime + durationMinutes). Excludes excludeEventId
        /// so updates don't collide with themselves (BRD 4.3.6 AC1).
        /// </summary>
        Task<bool> HasOverlapAsync(int venueId, DateTime eventDateTime, int durationMinutes, int? excludeEventId = null);

        /// <summary>Looks up the parent venue's TotalCapacity for capacity validation (BRD 4.3.6 AC2).</summary>
        Task<int?> GetVenueCapacityAsync(int venueId);

        Task<bool> VenueExistsAsync(int venueId);

        Task<bool> CategoryExistsAsync(int categoryId);

        /// <summary>Resolves display names for the given venue/category so DTOs don't need cross-module joins.</summary>
        Task<string> GetVenueNameAsync(int venueId);

        Task<string> GetCategoryNameAsync(int categoryId);
    }
}