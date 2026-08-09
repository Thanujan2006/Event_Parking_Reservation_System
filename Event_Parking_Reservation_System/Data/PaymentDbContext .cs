using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_Parking_Reservation_System.Data
{
    public class PaymentDbContext: DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>(p =>
            {
                p.ToTable("Payments");
                p.HasKey(x => x.PaymentId);
                p.Property(x => x.Amount).HasColumnType("decimal(10,2)");
                p.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                p.HasIndex(x => x.BookingId).IsUnique(); // 1 Payment per Booking (BRD Rule #2)
            });
        }
    }
}
