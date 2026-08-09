namespace Event_Parking_Reservation_System.Dtos.BookingDtos
{
    public class CreateBookingRequest
    {
        public int CustomerId { get; set; }

        public int EventId { get; set; }

        /// <summary>Must contain at least one element (BRD Business Rule #1).</summary>
        public List<int> SeatIds { get; set; } = new();

        /// <summary>Optional — parking reservation is not required to complete a booking.</summary>
        public int? ParkingSlotId { get; set; }
    }
}
