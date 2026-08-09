using Event_Parking_Reservation_System.Enums;

namespace Event_Parking_Reservation_System.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        /// <summary>Human-readable, system-generated. Format: BKG-YYYY-NNNNNN (BRD 4.6.4).</summary>
        public string BookingNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public int EventId { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        /// <summary>
        /// Set on creation to CreatedAt + configured hold-period minutes.
        /// Null once the booking is Confirmed (BRD Business Rule #6).
        /// </summary>
        public DateTime? HoldExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();

        /// <summary>Optional — a booking may have zero or exactly one parking reservation (BRD Module 5).</summary>
        public int? ParkingSlotId { get; set; }
    }
}
