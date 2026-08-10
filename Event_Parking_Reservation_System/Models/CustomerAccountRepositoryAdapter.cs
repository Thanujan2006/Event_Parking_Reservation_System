using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Event_Parking_Reservation_System.Models
{
    public class CustomerAccountRepositoryAdapter : ICustomerAccountRepository
    {
        private readonly AppDbContext _db;

        public CustomerAccountRepositoryAdapter(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CustomerAccountRecord?> GetByEmailAsync(string email)
        {
            var normalized = email.Trim().ToLower();
            var customer = await _db.Set<Customer>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email.ToLower() == normalized);
            return ToRecord(customer);
        }

        public async Task<CustomerAccountRecord?> GetByIdAsync(int customerId)
        {
            var customer = await _db.Set<Customer>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
            return ToRecord(customer);
        }

        public async Task<CustomerAccountRecord?> GetByEmailVerificationTokenHashAsync(string tokenHash)
        {
            var customer = await _db.Set<Customer>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Security.EmailVerificationToken != null &&
                    c.Security.EmailVerificationToken.TokenHash == tokenHash);
            return ToRecord(customer);
        }

        public async Task<CustomerAccountRecord?> GetByPasswordResetTokenHashAsync(string tokenHash)
        {
            var customer = await _db.Set<Customer>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Security.PasswordResetToken != null &&
                    c.Security.PasswordResetToken.TokenHash == tokenHash);
            return ToRecord(customer);
        }

        public async Task SetPasswordHashAsync(int customerId, string newPasswordHash)
        {
            var customer = await _db.Set<Customer>()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId)
                ?? throw new InvalidOperationException("Customer not found.");

            customer.PasswordHash = newPasswordHash;
            customer.UpdatedAt = System.DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task SaveSecurityStateAsync(int customerId, CustomerAccountSecurity security)
        {
            var customer = await _db.Set<Customer>()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId)
                ?? throw new InvalidOperationException("Customer not found.");

            customer.Security = security;
            customer.UpdatedAt = System.DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task MarkEmailVerifiedAsync(int customerId, CustomerAccountSecurity security)
        {
            var customer = await _db.Set<Customer>()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId)
                ?? throw new InvalidOperationException("Customer not found.");

            customer.Security = security;
            if (customer.Status == CustomerStatus.Unverified)
                customer.Status = CustomerStatus.Active;

            customer.UpdatedAt = System.DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        private static CustomerAccountRecord? ToRecord(Customer? customer) =>
            customer is null
                ? null
                : new CustomerAccountRecord(customer.CustomerId, customer.Email, customer.Security);
    }
}

