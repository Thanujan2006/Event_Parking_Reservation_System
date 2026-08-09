namespace Event_Parking_Reservation_System.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public CustomerStatus Status { get; set; } = CustomerStatus.Unverified;

        /// <summary>
        /// Authentication-owned security state. Configure this as an EF Core
        /// owned type with AuthEntityConfiguration.ConfigureSecurity(...).
        /// </summary>
        public CustomerAccountSecurity Security { get; set; } = CustomerAccountSecurity.NewUnverified();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }

    public enum CustomerStatus
    {
        Unverified = 0,
        Active = 1,
        Deactivated = 2
    }
}
