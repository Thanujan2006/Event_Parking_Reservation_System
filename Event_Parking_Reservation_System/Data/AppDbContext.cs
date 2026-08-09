using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_Parking_Reservation_System.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }


        public DbSet<Customer> Customers { get; set; }
        public DbSet<Booking> Bookings => Set<Booking>();

        public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();

        public DbSet<BookingSequence> BookingSequences => Set<BookingSequence>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<ParkingSlot> ParkingSlots => Set<ParkingSlot>();
        public DbSet<ParkingReservation> ParkingReservations => Set<ParkingReservation>();

        public DbSet<Event> Events => Set<Event>();
        public DbSet<Seat> Seats => Set<Seat>();

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Booking>(b =>
        //    {
        //        b.ToTable("Bookings");
        //        b.HasKey(x => x.BookingId);
        //        b.Property(x => x.BookingNumber).HasMaxLength(30).IsRequired();
        //        b.HasIndex(x => x.BookingNumber).IsUnique();
        //        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        //        b.HasMany(x => x.BookingSeats)
        //            .WithOne()
        //            .HasForeignKey(x => x.BookingId)
        //            .OnDelete(DeleteBehavior.Cascade);
        //    });

        //    modelBuilder.Entity<BookingSeat>(bs =>
        //    {
        //        bs.ToTable("BookingSeats");
        //        bs.HasKey(x => x.BookingSeatId);
        //        // A seat may only be attached to one *active* booking at a time; the
        //        // partial/filtered uniqueness (excluding Cancelled/Expired bookings) is
        //        // enforced in the repository transaction, not purely at the schema level,
        //        // since SQL Server unique constraints can't easily filter on a joined table's
        //        // status column.
        //        bs.HasIndex(x => x.SeatId);
        //    });

        //    modelBuilder.Entity<BookingSequence>(s =>
        //    {
        //        s.ToTable("BookingSequences");
        //        s.HasKey(x => x.Year);
        //    });
        //}


        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Payment>(p =>
        //    {
        //        p.ToTable("Payments");
        //        p.HasKey(x => x.PaymentId);
        //        p.Property(x => x.Amount).HasColumnType("decimal(10,2)");
        //        p.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        //        p.HasIndex(x => x.BookingId).IsUnique(); // 1 Payment per Booking (BRD Rule #2)
        //    });
        //}
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // SlotNumber unique per Event (BRD 4.5.12)
        //    modelBuilder.Entity<ParkingSlot>()
        //        .HasIndex(s => new { s.EventId, s.SlotNumber })
        //        .IsUnique();

        //    // BookingId is unique on ParkingReservations: at most one slot per booking (BRD 4.5.3/4.5.4).
        //    modelBuilder.Entity<ParkingReservation>()
        //        .HasIndex(r => r.BookingId)
        //        .IsUnique();

        //    // SlotId is unique on ParkingReservations: this is the hard backstop that makes
        //    // "one slot -> at most one reservation" atomic at the database level, even under
        //    // concurrent requests (BRD Business Rule #1 / Consolidated Rule #2).
        //    modelBuilder.Entity<ParkingReservation>()
        //        .HasIndex(r => r.SlotId)
        //        .IsUnique();

        //    modelBuilder.Entity<ParkingSlot>()
        //        .Property(s => s.Status)
        //        .HasConversion<string>()
        //        .HasMaxLength(20);
        //}
    }
}
