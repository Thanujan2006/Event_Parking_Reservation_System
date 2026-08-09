using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;
using Event_Parking_Reservation_System.Entities;

namespace Event_Parking_Reservation_System.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<Booking> Bookings => Set<Booking>();

        public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();

        public DbSet<BookingSequence> BookingSequences => Set<BookingSequence>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                // A seat may only be attached to one *active* booking at a time; the
                // partial/filtered uniqueness (excluding Cancelled/Expired bookings) is
                // enforced in the repository transaction, not purely at the schema level,
                // since SQL Server unique constraints can't easily filter on a joined table's
                // status column.
                bs.HasIndex(x => x.SeatId);
            });

            modelBuilder.Entity<BookingSequence>(s =>
            {
                s.ToTable("BookingSequences");
                s.HasKey(x => x.Year);
            });
        }
    }
}
