namespace Event_Parking_Reservation_System.Dtos.BookingDtos
{
    public class HoldStatusResponse
    {
        public int BookingId { get; set; }

        public string Status { get; set; } = string.Empty;

        /// <summary>Seconds remaining until the hold expires. 0 if already expired/confirmed/cancelled.</summary>
        public int RemainingSeconds { get; set; }
    }
}
