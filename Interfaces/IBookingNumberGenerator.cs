namespace Event_Parking_Reservation_System.Interfaces
{
    public interface IBookingNumberGenerator
    {
        Task<string> GenerateAsync();
    }
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
