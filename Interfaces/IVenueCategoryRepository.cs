
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event_Parking_Reservation_System.Models;

namespace Event_Parking_Reservation_System.Repositories
{
    public interface IVenueRepository
    {
        Task<Venue?> GetByIdAsync(int venueId);

        Task<IReadOnlyList<Venue>> GetAllAsync();

        Task<int> AddAsync(Venue venue);

        Task UpdateAsync(Venue venue);

        Task DeleteAsync(int venueId);

        /// <summary>
        /// Returns true if the venue has any event scheduled at or after now.
        /// </summary>
        Task<bool> HasUpcomingEventsAsync(int venueId);

        /// <summary>
        /// Returns venues with no event overlapping the given
        /// [startTime, endTime) window.
        /// </summary>
        Task<IReadOnlyList<Venue>> GetAvailableAsync(
            DateTime startTime,
            DateTime endTime);
    }

    public interface IEventCategoryRepository
    {
        Task<EventCategory?> GetByIdAsync(int categoryId);

        Task<IReadOnlyList<EventCategory>> GetAllAsync();

        Task<bool> NameExistsAsync(
            string name,
            int? excludeCategoryId = null);

        Task<int> AddAsync(EventCategory category);

        Task UpdateAsync(EventCategory category);

        Task DeleteAsync(int categoryId);

        /// <summary>
        /// Returns true if any event currently references this category.
        /// </summary>
        Task<bool> IsInUseAsync(int categoryId);
    }
}
