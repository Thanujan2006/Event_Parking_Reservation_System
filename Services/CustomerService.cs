using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using static Event_Parking_Reservation_System.Dtos.CustomerDtos.CustomerDtos;
using static Event_Parking_Reservation_System.Exceptions.CustomerExceptions;

namespace Event_Parking_Reservation_System.Services
{
    public class CustomerService: ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthService _authService;

        public CustomerService(
            ICustomerRepository repository,
            IPasswordHasher passwordHasher,
            IAuthService authService)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _authService = authService;
        }

        public async Task<RegisterCustomerResponse> RegisterAsync(RegisterCustomerRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (await _repository.EmailExistsAsync(normalizedEmail))
                throw new DuplicateEmailException(normalizedEmail);

            var customer = new Customer
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                PhoneNumber = request.PhoneNumber.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Status = CustomerStatus.Unverified,
                Security = CustomerAccountSecurity.NewUnverified(),
                CreatedAt = DateTime.UtcNow
            };

            var newId = await _repository.AddAsync(customer);

            // Authentication module creates, hashes, stores and sends the
            // verification token. This replaces the old disconnected
            // IVerificationEmailSender contract.
            await _authService.IssueEmailVerificationTokenForNewCustomerAsync(newId, customer.Email);

            return new RegisterCustomerResponse
            {
                CustomerId = newId,
                Email = customer.Email,
                Status = customer.Status.ToString()
            };
        }

        public async Task<CustomerProfileDto> GetOwnProfileAsync(int customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId)
                ?? throw new CustomerNotFoundException(customerId);
            return MapToProfileDto(customer);
        }

        public async Task<CustomerProfileDto> UpdateOwnProfileAsync(int customerId, UpdateCustomerProfileRequest request)
        {
            var customer = await _repository.GetByIdAsync(customerId)
                ?? throw new CustomerNotFoundException(customerId);

            customer.Name = request.Name.Trim();
            customer.PhoneNumber = request.PhoneNumber.Trim();
            customer.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(customer);
            return MapToProfileDto(customer);
        }

        public async Task<(IReadOnlyList<CustomerSummaryDto> Items, int TotalCount)> SearchAsync(
            string? searchTerm, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (items, total) = await _repository.SearchAsync(searchTerm, page, pageSize);
            var dtos = new List<CustomerSummaryDto>(items.Count);

            foreach (var c in items)
            {
                dtos.Add(new CustomerSummaryDto
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Email = c.Email,
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt
                });
            }

            return (dtos, total);
        }

        public async Task<CustomerDetailDto> GetDetailAsync(int customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId)
                ?? throw new CustomerNotFoundException(customerId);

            var activeBookings = await _repository.GetActiveFutureBookingIdsAsync(customerId);
            var totalBookings = await _repository.GetTotalBookingCountAsync(customerId);

            return new CustomerDetailDto
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Status = customer.Status.ToString(),
                CreatedAt = customer.CreatedAt,
                DeactivatedAt = customer.DeactivatedAt,
                TotalBookings = totalBookings,
                ActiveFutureBookings = activeBookings.Length
            };
        }

        public async Task DeactivateAsync(int customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId)
                ?? throw new CustomerNotFoundException(customerId);

            var activeBookingIds = await _repository.GetActiveFutureBookingIdsAsync(customerId);
            if (activeBookingIds.Length > 0)
                throw new ActiveBookingsExistException(activeBookingIds);

            customer.Status = CustomerStatus.Deactivated;
            customer.DeactivatedAt = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(customer);
        }

        public async Task ReactivateAsync(int customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId)
                ?? throw new CustomerNotFoundException(customerId);

            // Email verification is still mandatory after reactivation.
            customer.Status = customer.Security.EmailVerified
                ? CustomerStatus.Active
                : CustomerStatus.Unverified;
            customer.DeactivatedAt = null;
            customer.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(customer);
        }

        public async Task EnsureActiveForLoginAsync(int customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId)
                ?? throw new CustomerNotFoundException(customerId);

            if (customer.Status == CustomerStatus.Deactivated)
                throw new AccountDeactivatedException();
        }

        private static CustomerProfileDto MapToProfileDto(Customer c) => new()
        {
            CustomerId = c.CustomerId,
            Name = c.Name,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt
        };
    }
}
