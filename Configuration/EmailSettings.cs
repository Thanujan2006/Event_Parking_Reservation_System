namespace Event_Parking_Reservation_System.Configuration
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "Event & Parking Reservation System";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        /// <summary>When true, emails are only logged to console instead of actually sent —
        /// handy for local dev/viva demos without a real SMTP account configured.</summary>
        public bool SimulateOnly { get; set; } = true;
    }
}
