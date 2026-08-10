namespace Event_Parking_Reservation_System.Configuration
{
    public class BookingSettings
    {
        public const string SectionName = "Booking";

        public int HoldPeriodMinutes { get; set; } = 15;

        /// <summary>How often the expiry background job scans for expired holds (BRD 7.5).</summary>
        public int ExpiryScanIntervalSeconds { get; set; } = 60;
    }
}
