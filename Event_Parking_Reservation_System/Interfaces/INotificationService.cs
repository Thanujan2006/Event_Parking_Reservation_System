using Event_Parking_Reservation_System.Dtos.NotificationDtos;
using static Event_Parking_Reservation_System.Models.notification;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface INotificationService
    {

        /// <summary>
        /// Internal creation contract. Other modules (Booking, Payment, Event)
        /// depend on this interface directly and call it in-process —
        /// BRD 4.8.4: "NotificationService, Shared Internal Service Layer-ஆக
        /// அனைத்து Modules-லும் Inject செய்யப்பட்டு பயன்படுத்தப்படும்".
        /// This is intentionally separate from the public-facing
        /// INotificationQueryService so a module can depend on "create"
        /// without ever being able to reach "read someone else's inbox".
        /// </summary>
        public interface INotificationPublisher
        {
            Task<Notification> CreateAsync(int customerId, NotificationType type, string message);
        }

        /// <summary>
        /// Customer-facing read/update operations, backing the public
        /// HTTP endpoints in NotificationsController.
        /// </summary>
        public interface INotificationQueryService
        {
            /// <summary>
            /// BRD 4.8.11 — GET /api/notifications/customer/{customerId}.
            /// Newest-first. Throws NotificationAccessDeniedException if
            /// requestingCustomerId != customerId (BRD Rule #1).
            /// </summary>
            Task<IReadOnlyList<NotificationDto>> GetForCustomerAsync(int customerId, int requestingCustomerId);

            /// <summary>
            /// BRD 4.8.11 — PUT /api/notifications/{id}/read.
            /// Throws NotificationNotFoundException or
            /// NotificationAccessDeniedException as appropriate.
            /// </summary>
            Task MarkAsReadAsync(int notificationId, int requestingCustomerId);
        }

        /// <summary>
        /// Persistence contract — kept separate from the service so the
        /// service holds the business rules and the repository holds only
        /// data access (BRD 7.4: Controllers → Services → Repositories).
        /// </summary>
        public interface INotificationRepository
        {
            Task<Notification> AddAsync(Notification notification);
            Task<Notification?> GetByIdAsync(int notificationId);
            Task<IReadOnlyList<Notification>> GetByCustomerIdAsync(int customerId);
            Task SaveChangesAsync();
        }


    }
}
