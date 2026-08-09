
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event_Parking_Reservation_System.Dtos;

namespace Event_Parking_Reservation_System.Services
{
    public interface IVenueService
    {
        Task<VenueSummaryDto> CreateAsync(CreateVenueRequest request);

        Task<VenueDetailDto> GetDetailAsync(int venueId);

        Task<IReadOnlyList<VenueSummaryDto>> GetAllAsync();

        Task<VenueDetailDto> UpdateAsync(
            int venueId,
            UpdateVenueRequest request);

        Task DeleteAsync(int venueId);

        Task<IReadOnlyList<VenueSummaryDto>> GetAvailableAsync(
            DateTime startTime,
            DateTime endTime);
    }

    public interface IEventCategoryService
    {
        Task<CategoryDto> CreateAsync(
            CreateCategoryRequest request);

        Task<IReadOnlyList<CategoryDto>> GetAllAsync();

        Task<CategoryDto> UpdateAsync(
            int categoryId,
            UpdateCategoryRequest request);

        Task DeleteAsync(int categoryId);
    }
}

