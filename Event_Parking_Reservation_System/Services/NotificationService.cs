using Event_Parking_Reservation_System.Models;
using static Event_Parking_Reservation_System.Models.notification;

namespace Event_Parking_Reservation_System_Services
{
    public class NotificationService
    {
        /// <summary>
        /// Common customer contract. Other modules (Booking, Payment, Event)
        /// depend on this interface directly and call it.
        /// </summary>
        public interface INotificationPublisher
        {
            Task<Notification> CreateAsync(int customerId, NotificationType type, string message);
            Task<Notification> CreateAsync(int customerId, Amazon.SimpleSystemsManagement.NotificationType type, string message);
        }

        /// <summary>
        /// Customer-facing read/update operations, backing the public
        /// HTTP endpoints in NotificationController.
        /// </summary>
        public interface INotificationQueryService
        {
            Task<IReadOnlyList<Notification>> GetForCustomerAsync(int customerId, int requestingCustomerId);
            Task MarkAsReadAsync(int notificationId, int requestingCustomerId);
        }

        /// <summary>
        /// Persistence contract - repository handles data access logic.
        /// </summary>
        public interface INotificationRepository
        {
            Task<Notification> AddAsync(Notification notification);
            Task<Notification> GetByIdAsync(int notificationId);
            Task<IReadOnlyList<Notification>> GetByCustomerAsync(int customerId);
            Task SaveChangesAsync();
        }
    }
}
