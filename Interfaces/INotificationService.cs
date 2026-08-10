using Event_Parking_Reservation_System.Dtos.NotificationDtos;
using static Event_Parking_Reservation_System.Models.notification;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface INotificationService
    {
        public interface INotificationPublisher
        {
            Task<Notification> CreateAsync(int customerId, NotificationType type, string message);
        }

        public interface INotificationQueryService
        {
            Task<IReadOnlyList<NotificationDto>> GetForCustomerAsync(int customerId, int requestingCustomerId);
            Task MarkAsReadAsync(int notificationId, int requestingCustomerId);
        }

        public interface INotificationRepository
        {
            Task<Notification> AddAsync(Notification notification);
            Task<Notification?> GetByIdAsync(int notificationId);
            Task<IReadOnlyList<Notification>> GetByCustomerIdAsync(int customerId);
            Task SaveChangesAsync();
        }
    }
}
