using Event_Parking_Reservation_System.Dtos;
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Models;
using Event_Parking_Reservation_System.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Event_Parking_Reservation_System.Services.IVenueCategoryRepository;


namespace Event_Parking_Reservation_System.Services
{
    /// <summary>
    /// Encodes Module 2 business rules (BRD 4.2.5–4.2.6):
    /// - Venue cannot be deleted while it has upcoming scheduled events (AC1)
    /// - Availability check driven purely by [startTime, endTime) overlap (AC2)
    /// - Category cannot be deleted while in use by an event (AC3)
    /// - Category name must be unique
    /// </summary>


    public class VenueCategoryService:IVenueCategoryRepository {
        public class VenueService : IVenueService
        {
            private readonly IVenueRepository _repository;

            public VenueService(IVenueRepository repository)
            {
                _repository = repository;
            }

            public async Task<VenueSummaryDto> CreateAsync(CreateVenueRequest request)
            {
                var venue = new Venue
                {
                    Name = request.Name.Trim(),
                    Address = request.Address.Trim(),
                    TotalCapacity = request.TotalCapacity,
                    CreatedAt = DateTime.UtcNow
                };

                var id = await _repository.AddAsync(venue);

                return new VenueSummaryDto
                {
                    VenueId = id,
                    Name = venue.Name,
                    TotalCapacity = venue.TotalCapacity
                };
            }

            public async Task<VenueDetailDto> GetDetailAsync(int venueId)
            {
                var venue = await _repository.GetByIdAsync(venueId)
                    ?? throw new VenueNotFoundException(venueId);

                return MapToDetailDto(venue);
            }

            public async Task<IReadOnlyList<VenueSummaryDto>> GetAllAsync()
            {
                var venues = await _repository.GetAllAsync();
                return venues.Select(v => new VenueSummaryDto
                {
                    VenueId = v.VenueId,
                    Name = v.Name,
                    TotalCapacity = v.TotalCapacity
                }).ToList();
            }

            public async Task<VenueDetailDto> UpdateAsync(int venueId, UpdateVenueRequest request)
            {
                var venue = await _repository.GetByIdAsync(venueId)
                    ?? throw new VenueNotFoundException(venueId);

                venue.Name = request.Name.Trim();
                venue.Address = request.Address.Trim();
                venue.TotalCapacity = request.TotalCapacity;
                venue.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(venue);

                return MapToDetailDto(venue);
            }

            public async Task DeleteAsync(int venueId)
            {
                var venue = await _repository.GetByIdAsync(venueId)
                    ?? throw new VenueNotFoundException(venueId);

                // AC1: block delete while upcoming events are scheduled.
                if (await _repository.HasUpcomingEventsAsync(venueId))
                {
                    throw new VenueHasUpcomingEventsException(venueId);
                }

                await _repository.DeleteAsync(venueId);
            }

            public async Task<IReadOnlyList<VenueSummaryDto>> GetAvailableAsync(DateTime startTime, DateTime endTime)
            {
                // BRD 4.2.12: startTime < endTime is mandatory.
                if (startTime >= endTime)
                {
                    throw new InvalidTimeRangeException();
                }

                var venues = await _repository.GetAvailableAsync(startTime, endTime);

                return venues.Select(v => new VenueSummaryDto
                {
                    VenueId = v.VenueId,
                    Name = v.Name,
                    TotalCapacity = v.TotalCapacity
                }).ToList();
            }

            private static VenueDetailDto MapToDetailDto(Venue v) => new()
            {
                VenueId = v.VenueId,
                Name = v.Name,
                Address = v.Address,
                TotalCapacity = v.TotalCapacity,
                CreatedAt = v.CreatedAt
            };
        }

        public class EventCategoryService : IEventCategoryService
        {
            private readonly IEventCategoryRepository _repository;

            public EventCategoryService(IEventCategoryRepository repository)
            {
                _repository = repository;
            }

            public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
            {
                var name = request.Name.Trim();

                if (await _repository.NameExistsAsync(name))
                {
                    throw new DuplicateCategoryNameException(name);
                }

                var category = new EventCategory { Name = name, CreatedAt = DateTime.UtcNow };
                var id = await _repository.AddAsync(category);

                return new CategoryDto { CategoryId = id, Name = category.Name };
            }

            public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
            {
                var categories = await _repository.GetAllAsync();
                return categories.Select(c => new CategoryDto { CategoryId = c.CategoryId, Name = c.Name }).ToList();
            }

            public async Task<CategoryDto> UpdateAsync(int categoryId, UpdateCategoryRequest request)
            {
                var category = await _repository.GetByIdAsync(categoryId)
                    ?? throw new CategoryNotFoundException(categoryId);

                var name = request.Name.Trim();

                if (await _repository.NameExistsAsync(name, excludeCategoryId: categoryId))
                {
                    throw new DuplicateCategoryNameException(name);
                }

                category.Name = name;
                await _repository.UpdateAsync(category);

                return new CategoryDto { CategoryId = category.CategoryId, Name = category.Name };
            }

            public async Task DeleteAsync(int categoryId)
            {
                var category = await _repository.GetByIdAsync(categoryId)
                    ?? throw new CategoryNotFoundException(categoryId);

                // AC3: block delete while the category is referenced by an event.
                if (await _repository.IsInUseAsync(categoryId))
                {
                    throw new CategoryInUseException(categoryId);
                }

                await _repository.DeleteAsync(categoryId);
            }
        }
  
    }
}