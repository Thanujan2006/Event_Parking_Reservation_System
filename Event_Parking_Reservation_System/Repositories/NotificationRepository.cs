using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using static Event_Parking_Reservation_System.Models.notification;
using static Event_Parking_Reservation_System_Services.NotificationService;

namespace Event_Parking_Reservation_System.Reposotires
{
    
        public class NotificationRepository : INotificationRepository
        {
            private readonly DbContext _context;
            private readonly DbSet<Notification> _notifications;

            public NotificationRepository(DbContext context)
            {
                _context = context;
                _notifications = context.Set<Notification>();
            }

            public async Task<Notification> AddAsync(Notification notification)
            {
                await _notifications.AddAsync(notification);
                return notification;
            }

            public Task<Notification?> GetByIdAsync(int notificationId) =>
                _notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            public async Task<IReadOnlyList<Notification>> GetByCustomerIdAsync(int customerId) =>
                await _notifications
                    .Where(n => n.CustomerId == customerId)
                    .ToListAsync();

            public Task SaveChangesAsync() => _context.SaveChangesAsync();

        public Task<IReadOnlyList<Notification>> GetByCustomerAsync(int customerId)
        {
            throw new NotImplementedException();
        }
    }

        /// <summary>
        /// EF Core mapping for the Notifications table (BRD 4.8.10).
        /// Register via modelBuilder.ApplyConfiguration(new NotificationConfiguration()).
        /// </summary>
        public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
        {
            public void Configure(EntityTypeBuilder<Notification> builder)
            {
                builder.ToTable("Notifications");

                builder.HasKey(n => n.NotificationId);

                builder.Property(n => n.CustomerId)
                    .IsRequired();

                builder.Property(n => n.Type)
                    .HasConversion<string>()
                    .IsRequired();

                builder.Property(n => n.Message)
                    .HasMaxLength(500)
                    .IsRequired();

                builder.Property(n => n.IsRead)
                    .HasDefaultValue(false);

                builder.Property(n => n.CreatedAt)
                    .IsRequired();

                // Fast "newest-first for this customer" queries (BRD AC2).
                builder.HasIndex(n => new { n.CustomerId, n.CreatedAt });
            }
        }




    
}
