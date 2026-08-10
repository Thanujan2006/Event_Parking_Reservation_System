namespace Event_Parking_Reservation_System.Dtos.BookingDtos
{
    public class BookingDtos
    {
        public int BookingId { get; set; }

        public string BookingNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public int EventId { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? HoldExpiresAt { get; set; }

        public List<int> SeatIds { get; set; } = new();

        public int? ParkingSlotId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
