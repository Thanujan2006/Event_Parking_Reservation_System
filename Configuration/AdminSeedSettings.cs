namespace Event_Parking_Reservation_System.Configuration
{
    public class AdminSeedSettings
    {
        public const string SectionName = "AdminSeed";

        public string Email { get; set; } = "admin@eventpark.com";
        public string Password { get; set; } = "Admin@123";
        public string Name { get; set; } = "System Admin";
        public string PhoneNumber { get; set; } = "0770000000";
    }
}
