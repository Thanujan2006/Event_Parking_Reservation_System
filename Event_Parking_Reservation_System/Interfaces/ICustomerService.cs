using static Event_Parking_Reservation_System.Dtos.CustomerDtos.CustomerDtos;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface ICustomerService
    {
        Task<RegisterCustomerResponse> RegisterAsync(RegisterCustomerRequest request);

        Task<CustomerProfileDto> GetOwnProfileAsync(int customerId);

        Task<CustomerProfileDto> UpdateOwnProfileAsync(int customerId, UpdateCustomerProfileRequest request);

        Task<(System.Collections.Generic.IReadOnlyList<CustomerSummaryDto> Items, int TotalCount)> SearchAsync(
            string? searchTerm, int page, int pageSize);

        Task<CustomerDetailDto> GetDetailAsync(int customerId);

        Task DeactivateAsync(int customerId);

        Task ReactivateAsync(int customerId);

        /// <summary>Throws AccountDeactivatedException if the customer's status is Deactivated. Used during login.</summary>
        Task EnsureActiveForLoginAsync(int customerId);

    }
}
