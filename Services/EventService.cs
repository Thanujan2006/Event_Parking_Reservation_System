using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event_Parking_Reservation_System.Dtos;
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Models;
using Event_Parking_Reservation_System.Repositories;

namespace Event_Parking_Reservation_System.Services
{
    /// <summary>
    /// Encodes Module 3 business rules (BRD 4.3.5–4.3.6):
    /// - Venue/Category must exist
    /// - New/updated event time cannot overlap another event at the same venue (AC1)
    /// - Event capacity cannot exceed the venue's total capacity (AC2)
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IEventRepository _repository;
        private readonly SeatRepository _seatRepository;

        public EventService(IEventRepository repository, SeatRepository seatRepository)
        {
            _repository = repository;
            _seatRepository = seatRepository;
        }

        public async Task<EventDetailDto> CreateAsync(CreateEventRequest request)
        {
            await ValidateReferencesAsync(request.VenueId, request.CategoryId);
            await ValidateCapacityAsync(request.VenueId, request.Capacity);
            await ValidateNoOverlapAsync(request.VenueId, request.EventDateTime, request.DurationMinutes);

            var ev = new Event
            {
                Name = request.Name.Trim(),
                VenueId = request.VenueId,
                CategoryId = request.CategoryId,
                EventDateTime = request.EventDateTime,
                DurationMinutes = request.DurationMinutes,
                TicketPrice = request.TicketPrice,
                Capacity = request.Capacity,
                CreatedAt = DateTime.UtcNow
            };

            var id = await _repository.AddAsync(ev);
            return await GetDetailAsync(id);
        }

        public async Task<EventDetailDto> GetDetailAsync(int eventId)
        {
            var ev = await _repository.GetByIdAsync(eventId)
                ?? throw new EventNotFoundException(eventId);

            return await MapToDetailDtoAsync(ev);
        }

        public async Task<PagedResult<EventSummaryDto>> SearchAsync(EventSearchQuery query)
        {
            var (items, total) = await _repository.SearchAsync(
                query.Name, query.Date, query.VenueId, query.CategoryId, query.Page, query.PageSize);

            var dtos = new List<EventSummaryDto>(items.Count);
            foreach (var ev in items)
            {
                dtos.Add(new EventSummaryDto
                {
                    EventId = ev.EventId,
                    Name = ev.Name,
                    VenueName = await _repository.GetVenueNameAsync(ev.VenueId),
                    CategoryName = await _repository.GetCategoryNameAsync(ev.CategoryId),
                    EventDateTime = ev.EventDateTime,
                    EventDate = ev.EventDateTime.ToString("yyyy-MM-dd"),
                    StartTime = ev.EventDateTime.ToString("HH:mm:ss"),
                    TicketPrice = ev.TicketPrice,
                    Capacity = ev.Capacity,
                    SeatsAvailable = await ResolveSeatsAvailableAsync(ev)
                });
            }

            return new PagedResult<EventSummaryDto> { Items = dtos, TotalCount = total };
        }

        public async Task<EventDetailDto> UpdateAsync(int eventId, UpdateEventRequest request)
        {
            var ev = await _repository.GetByIdAsync(eventId)
                ?? throw new EventNotFoundException(eventId);

            await ValidateReferencesAsync(request.VenueId, request.CategoryId);
            await ValidateCapacityAsync(request.VenueId, request.Capacity);
            await ValidateNoOverlapAsync(request.VenueId, request.EventDateTime, request.DurationMinutes, excludeEventId: eventId);

            ev.Name = request.Name.Trim();
            ev.VenueId = request.VenueId;
            ev.CategoryId = request.CategoryId;
            ev.EventDateTime = request.EventDateTime;
            ev.DurationMinutes = request.DurationMinutes;
            ev.TicketPrice = request.TicketPrice;
            ev.Capacity = request.Capacity;
            ev.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(ev);

            return await MapToDetailDtoAsync(ev);
        }

        public async Task DeleteAsync(int eventId)
        {
            _ = await _repository.GetByIdAsync(eventId)
                ?? throw new EventNotFoundException(eventId);

            await _repository.DeleteAsync(eventId);
        }

        private async Task ValidateReferencesAsync(int venueId, int categoryId)
        {
            if (!await _repository.VenueExistsAsync(venueId))
            {
                throw new VenueNotFoundException(venueId);
            }

            if (!await _repository.CategoryExistsAsync(categoryId))
            {
                throw new CategoryNotFoundException(categoryId);
            }
        }

        private async Task ValidateCapacityAsync(int venueId, int eventCapacity)
        {
            // AC2: event capacity must not exceed the venue's total capacity.
            var venueCapacity = await _repository.GetVenueCapacityAsync(venueId);
            if (venueCapacity.HasValue && eventCapacity > venueCapacity.Value)
            {
                throw new EventCapacityExceedsVenueException(eventCapacity, venueCapacity.Value);
            }
        }

        private async Task ValidateNoOverlapAsync(
            int venueId, DateTime eventDateTime, int durationMinutes, int? excludeEventId = null)
        {
            // AC1: same-venue schedule overlap check only (BRD 4.3.4 — no
            // cross-venue restriction).
            if (await _repository.HasOverlapAsync(venueId, eventDateTime, durationMinutes, excludeEventId))
            {
                throw new VenueScheduleOverlapException(venueId);
            }
        }

        private async Task<EventDetailDto> MapToDetailDtoAsync(Event ev)
        {
            var end = ev.EventDateTime.AddMinutes(ev.DurationMinutes);
            return new EventDetailDto
            {
                EventId = ev.EventId,
                Name = ev.Name,
                VenueId = ev.VenueId,
                VenueName = await _repository.GetVenueNameAsync(ev.VenueId),
                CategoryId = ev.CategoryId,
                CategoryName = await _repository.GetCategoryNameAsync(ev.CategoryId),
                EventDateTime = ev.EventDateTime,
                EventDate = ev.EventDateTime.ToString("yyyy-MM-dd"),
                StartTime = ev.EventDateTime.ToString("HH:mm:ss"),
                EndTime = end.ToString("HH:mm:ss"),
                DurationMinutes = ev.DurationMinutes,
                TicketPrice = ev.TicketPrice,
                Capacity = ev.Capacity,
                SeatsAvailable = await ResolveSeatsAvailableAsync(ev),
                CreatedAt = ev.CreatedAt
            };
        }

        private async Task<int> ResolveSeatsAvailableAsync(Event ev)
        {
            var seatCount = await _seatRepository.GetSeatCountForEventAsync(ev.EventId);
            if (seatCount == 0)
                return ev.Capacity;

            return await _seatRepository.CountAvailableSeatsForEventAsync(ev.EventId);
        }
    }
}
