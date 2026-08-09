using Event_Parking_Reservation_System.Dtos.BookingDtos;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface IBookingService
    {
        Task<CreateBookingResponse> CreateBookingAsync(CreateBookingRequest request);

        Task<BookingDtos> GetByIdAsync(int bookingId, int requestingCustomerId, bool isAdmin);

        Task<System.Collections.Generic.IReadOnlyList<BookingDtos>> GetCustomerHistoryAsync(int customerId);

        Task<System.Collections.Generic.IReadOnlyList<BookingDtos>> GetByEventIdAsync(int eventId);

        Task<HoldStatusResponse> GetHoldStatusAsync(int bookingId);

        Task CancelAsync(int bookingId, int requestingCustomerId, bool isAdmin);

        /// <summary>
        /// Called by the Payment module (Module 7) once payment succeeds, to flip
        /// Pending -> Confirmed and clear the hold (BRD Business Rule #6).
        /// </summary>
        Task ConfirmAsync(int bookingId);

        /// <summary>Invoked periodically by the background expiry job (BRD Business Rule #4).</summary>
        Task<int> ExpireOverdueHoldsAsync();
    }
}
