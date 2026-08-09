using Event_Parking_Reservation_System.Enums;
using Event_Parking_Reservation_System.Entities;

namespace Event_Parking_Reservation_System.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        /// <summary>Unique — enforces "1 Payment per Booking" (BRD Business Rule #2).</summary>
        public int BookingId { get; set; }

        /// <summary>Seats total + Parking fee, recalculated server-side at payment time.</summary>
        public decimal Amount { get; set; }
        public paymentStatus Status { get; set; } = paymentStatus.Completed;

        public DateTime PaidAt { get; set; }
    }
}
