using Event_Parking_Reservation_System.Interfaces;

namespace Event_Parking_Reservation_System.Services
{
    public class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
