using System.Threading.Tasks;
using Event_Parking_Reservation_System.Dtos;

namespace Event_Parking_Reservation_System.Services
{
    public interface IEventService
    {
        Task<EventDetailDto> CreateAsync(CreateEventRequest request);
        Task<EventDetailDto> GetDetailAsync(int eventId);
        Task<PagedResult<EventSummaryDto>> SearchAsync(EventSearchQuery query);
        Task<EventDetailDto> UpdateAsync(int eventId, UpdateEventRequest request);
        Task DeleteAsync(int eventId);
    }
}