using Event_Parking_Reservation_System.Models;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking> CreateWithHoldAsync(Booking booking, IEnumerable<int> seatIds, int? parkingSlotId);

        Task<Booking?> GetByIdAsync(int bookingId);

        Task<IReadOnlyList<Booking>> GetByCustomerIdAsync(int customerId);

        Task<IReadOnlyList<Booking>> GetByEventIdAsync(int eventId);

        /// <summary>
        /// Cancels a booking and releases its seats/parking slot back to Available,
        /// atomically (BRD Business Rule #2).
        /// </summary>
        Task CancelAsync(int bookingId);

        /// <summary>
        /// Marks the given bookings Expired and releases their seats/parking, atomically,
        /// used by the background expiry job (BRD Business Rule #4).
        /// </summary>
        Task<int> ExpireHeldBookingsAsync(DateTime now);

        /// <summary>Flips status to Confirmed and clears HoldExpiresAt (BRD Business Rule #6).</summary>
        Task ConfirmAsync(int bookingId);

        Task<IReadOnlyList<Booking>> GetPendingWithExpiredHoldsAsync(DateTime now);
    }
}

