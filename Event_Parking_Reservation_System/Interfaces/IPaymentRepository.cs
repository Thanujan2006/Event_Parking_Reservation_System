using Event_Parking_Reservation_System.Models;
using Event_Parking_Reservation_System.Entities;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByBookingIdAsync(int bookingId);

        Task<Payment?> GetByIdAsync(int paymentId);

        Task<IReadOnlyList<Payment>> GetByCustomerBookingIdsAsync(IEnumerable<int> bookingIds);

        /// <summary>
        /// Inserts the payment inside a transaction that re-checks (with a row lock) that
        /// no Payment already exists for this BookingId, so two concurrent "Pay Now" clicks
        /// can't both succeed (BRD Business Rule #2).
        /// </summary>
        Task<Payment> CreateAsync(Payment payment);
    }
}
