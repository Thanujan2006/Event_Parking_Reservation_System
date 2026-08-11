using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_Parking_Reservation_System.Repositories
{
    
        public class CustomerRepository : ICustomerRepository
        {
            private readonly AppDbContext _db;

            public CustomerRepository(AppDbContext db)
            {
                _db = db;
            }

            // methods...
        


        public async Task<Customer?> GetByIdAsync(int customerId)
        {
            return await _db.Set<Customer>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _db.Set<Customer>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _db.Set<Customer>()
                .AnyAsync(c => c.Email.ToLower() == email.ToLower());
        }

        public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> SearchAsync(
            string? searchTerm, int page, int pageSize)
        {
            var query = _db.Set<Customer>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip(Math.Max(0, page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> AddAsync(Customer customer)
        {
            await _db.Set<Customer>().AddAsync(customer);
            await _db.SaveChangesAsync();
            return customer.CustomerId;
        }

        public async Task UpdateAsync(Customer customer)
        {
            _db.Set<Customer>().Update(customer);
            await _db.SaveChangesAsync();
        }

        public async Task<int[]> GetActiveFutureBookingIdsAsync(int customerId)
        {
            // Booking entity lives in Module 6 (Booking & Payment Management).
            // Queried here via raw SQL against the shared schema so this module
            // does not take a hard compile-time dependency on the Booking module.
            var ids = await _db.Database
                .SqlQuery<int>($@"
                    SELECT BookingId
                    FROM Bookings
                    WHERE CustomerId = {customerId}
                      AND Status = 'Confirmed'
                      AND EventDateTime > GETUTCDATE()")
                .ToListAsync();

            return ids.ToArray();
        }

        public async Task<int> GetTotalBookingCountAsync(int customerId)
        {
            var count = await _db.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(*) AS Value
                    FROM Bookings
                    WHERE CustomerId = {customerId}")
                .FirstOrDefaultAsync();

            return count;
        }
    }
}

