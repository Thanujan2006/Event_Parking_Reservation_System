
using Event_Parking_Reservation_System.DTOs;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface ISeatServices
    {
        Task GenerateSeatMapAsync(int eventId, GenerateSeatMapRequest request);
        
        Task<SeatMapDto> GetSeatMapAsync(int eventId);

        /// <summary>AC1 — race-safe hold; throws SeatsUnavailableException listing seats that were already taken.</summary>
        Task<HoldSeatsResult> HoldSeatsAsync(int bookingId, HoldSeatsRequest request);

        Task ConfirmSeatsAsync(int bookingId);

        Task ReleaseHeldSeatsAsync(int bookingId);

        /// <summary>AC3 — throws SeatHasActiveBookingException if the seat is Held or Booked.</summary>
        Task DeleteSeatAsync(int seatId);
    }
}
