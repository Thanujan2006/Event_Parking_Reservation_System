using Event_Parking_Reservation_System.DTOs;

namespace Event_Parking_Reservation_System.Services
{
    public interface IParkingService
    {
        Task<List<ParkingSlotDto>> GetLayoutAsync(int eventId);

        Task<List<ParkingSlotDto>> CreateLayoutAsync(int eventId, ParkingLayoutCreateDto dto);

        Task<ParkingSlotDto> UpdateSlotAsync(int eventId, int slotId, ParkingSlotUpdateDto dto);

        Task RemoveSlotAsync(int eventId, int slotId);

        Task<ParkingReservationDto> ReserveAsync(int bookingId, ParkingReserveDto dto);

        Task RemoveReservationAsync(int bookingId);
    }
}
