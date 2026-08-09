namespace Event_Parking_Reservation_System.Dtos.BookingDtos
{
    public class CreateBookingResponse
    {
        public int BookingId { get; set; }

        public string BookingNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime? HoldExpiresAt { get; set; }
    }
}
