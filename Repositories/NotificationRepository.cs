using Event_Parking_Reservation_System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Event_Parking_Reservation_System.Interfaces.INotificationService;
using static Event_Parking_Reservation_System.Models.notification;

namespace Event_Parking_Reservation_System.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Notification> _notifications;

        public NotificationRepository(AppDbContext context)
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
    }

    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.NotificationId);

            builder.Property(n => n.CustomerId).IsRequired();
            builder.Property(n => n.Type).HasConversion<string>().IsRequired();
            builder.Property(n => n.Message).HasMaxLength(500).IsRequired();
            builder.Property(n => n.IsRead).HasDefaultValue(false);
            builder.Property(n => n.CreatedAt).IsRequired();
            builder.Ignore(n => n.Id);

            builder.HasIndex(n => new { n.CustomerId, n.CreatedAt });
        }
    }
}
