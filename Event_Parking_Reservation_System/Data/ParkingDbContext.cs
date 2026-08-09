
using Microsoft.EntityFrameworkCore;
using Event_Parking_Reservation_System.Models;


namespace Event_Parking_Reservation_System.Data
{
    public class ParkingDbContext : DbContext
    {
        public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options) { }

        public DbSet<ParkingSlot> ParkingSlots => Set<ParkingSlot>();
        public DbSet<ParkingReservation> ParkingReservations => Set<ParkingReservation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // SlotNumber unique per Event (BRD 4.5.12)
            modelBuilder.Entity<ParkingSlot>()
                .HasIndex(s => new { s.EventId, s.SlotNumber })
                .IsUnique();

            // BookingId is unique on ParkingReservations: at most one slot per booking (BRD 4.5.3/4.5.4).
            modelBuilder.Entity<ParkingReservation>()
                .HasIndex(r => r.BookingId)
                .IsUnique();

            // SlotId is unique on ParkingReservations: this is the hard backstop that makes
            // "one slot -> at most one reservation" atomic at the database level, even under
            // concurrent requests (BRD Business Rule #1 / Consolidated Rule #2).
            modelBuilder.Entity<ParkingReservation>()
                .HasIndex(r => r.SlotId)
                .IsUnique();

            modelBuilder.Entity<ParkingSlot>()
                .Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        }
    }
}
