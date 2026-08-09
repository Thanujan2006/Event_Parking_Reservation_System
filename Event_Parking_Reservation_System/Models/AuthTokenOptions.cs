namespace Event_Parking_Reservation_System.Models
{
    public class AuthTokenOptions
    {
        public int EmailVerificationLifetimeHours { get; set; } = 24;

        // BRD default range is 30-60 minutes; 45 is the configured midpoint default.
        public int PasswordResetLifetimeMinutes { get; set; } = 45;

        public TimeSpan EmailVerificationLifetime => TimeSpan.FromHours(EmailVerificationLifetimeHours);
        public TimeSpan PasswordResetLifetime => TimeSpan.FromMinutes(PasswordResetLifetimeMinutes);
    }
}
