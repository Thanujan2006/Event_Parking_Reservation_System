using Event_Parking_Reservation_System.Models;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int customerId);

        Task<Customer?> GetByEmailAsync(string email);

        Task<bool> EmailExistsAsync(string email);

        Task<(System.Collections.Generic.IReadOnlyList<Customer> Items, int TotalCount)> SearchAsync(
            string? searchTerm, int page, int pageSize);

        Task<int> AddAsync(Customer customer);

        Task UpdateAsync(Customer customer);

        /// <summary>Returns booking IDs for active (future, non-cancelled) bookings for this customer.</summary>
        Task<int[]> GetActiveFutureBookingIdsAsync(int customerId);

        Task<int> GetTotalBookingCountAsync(int customerId);
    }
}
