using Event_Parking_Reservation_System.Models;
using Event_Parking_Reservation_System.Repositories;
using Microsoft.EntityFrameworkCore;
using static Event_Parking_Reservation_System.Models.notification;

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
        public DbSet<Venue> Venues => Set<Venue>();
        public DbSet<EventCategory> EventCategories => Set<EventCategory>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new CustomerEntityCnfiguration().Configure(modelBuilder.Entity<Customer>());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());

            modelBuilder.Entity<Venue>(v =>
            {
                v.ToTable("Venues");
                v.HasKey(x => x.VenueId);
                v.Property(x => x.Name).HasMaxLength(200).IsRequired();
                v.Property(x => x.Address).HasMaxLength(500).IsRequired();
            });

            modelBuilder.Entity<EventCategory>(c =>
            {
                c.ToTable("EventCategories");
                c.HasKey(x => x.CategoryId);
                c.Property(x => x.Name).HasMaxLength(100).IsRequired();
                c.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<Event>(e =>
            {
                e.ToTable("Events");
                e.HasKey(x => x.EventId);
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.TicketPrice).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<Seat>(s =>
            {
                s.ToTable("Seats");
                s.HasKey(x => x.SeatId);
                s.Property(x => x.SeatNumber).HasMaxLength(20).IsRequired();
                s.Property(x => x.PriceOverride).HasColumnType("decimal(10,2)");
                s.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                s.HasIndex(x => new { x.EventId, x.SeatNumber }).IsUnique();
            });

            modelBuilder.Entity<Booking>(b =>
            {
                b.ToTable("Bookings");
                b.HasKey(x => x.BookingId);
                b.Property(x => x.BookingNumber).HasMaxLength(30).IsRequired();
                b.HasIndex(x => x.BookingNumber).IsUnique();
                b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                b.HasMany(x => x.BookingSeats)
                    .WithOne()
                    .HasForeignKey(x => x.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BookingSeat>(bs =>
            {
                bs.ToTable("BookingSeats");
                bs.HasKey(x => x.BookingSeatId);
                bs.HasIndex(x => x.SeatId);
            });

            modelBuilder.Entity<BookingSequence>(s =>
            {
                s.ToTable("BookingSequences");
                s.HasKey(x => x.Id);
                s.HasIndex(x => x.Year).IsUnique();
            });

            modelBuilder.Entity<Payment>(p =>
            {
                p.ToTable("Payments");
                p.HasKey(x => x.PaymentId);
                p.Property(x => x.Amount).HasColumnType("decimal(10,2)");
                p.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                p.HasIndex(x => x.BookingId).IsUnique();
            });

            modelBuilder.Entity<ParkingSlot>(s =>
            {
                s.ToTable("ParkingSlots");
                s.HasKey(x => x.SlotId);
                s.Property(x => x.SlotNumber).HasMaxLength(20).IsRequired();
                s.Property(x => x.Fee).HasColumnType("decimal(10,2)");
                s.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                s.HasIndex(x => new { x.EventId, x.SlotNumber }).IsUnique();
            });

            modelBuilder.Entity<ParkingReservation>(r =>
            {
                r.ToTable("ParkingReservations");
                r.HasKey(x => x.ReservationId);
                r.Property(x => x.FeeAtReservation).HasColumnType("decimal(10,2)");
                r.HasIndex(x => x.BookingId).IsUnique();
                r.HasIndex(x => x.SlotId).IsUnique();
            });
        }
    }
}
