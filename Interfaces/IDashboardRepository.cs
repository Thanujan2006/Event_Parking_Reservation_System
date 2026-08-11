namespace Event_Parking_Reservation_System.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> GetTotalEventsAsync();
        Task<int> GetTotalBookingsAsync();
        Task<int> GetTotalAvailableSeatsAsync();
        Task<int> GetTotalOccupiedParkingSlotsAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetTotalCustomersAsync();
    }
}