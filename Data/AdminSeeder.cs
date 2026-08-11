using Event_Parking_Reservation_System.Configuration;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Event_Parking_Reservation_System.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var settings = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedSettings>>().Value;

            var email = settings.Email.Trim().ToLowerInvariant();
            var existing = await db.Customers.FirstOrDefaultAsync(c => c.Email == email);

            if (existing is null)
            {
                db.Customers.Add(new Customer
                {
                    Name = settings.Name.Trim(),
                    Email = email,
                    PhoneNumber = settings.PhoneNumber.Trim(),
                    PasswordHash = hasher.Hash(settings.Password),
                    Status = CustomerStatus.Active,
                    Security = CustomerAccountSecurity.NewActive(),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                // Keep hardcoded admin credentials in sync for beginner/demo use.
                existing.Role = "Admin";
                existing.Status = CustomerStatus.Active;
                existing.PasswordHash = hasher.Hash(settings.Password);
                existing.Name = settings.Name.Trim();
                existing.PhoneNumber = settings.PhoneNumber.Trim();
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
        }
    }
}
