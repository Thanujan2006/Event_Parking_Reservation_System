using Event_Parking_Reservation_System.Dtos.NotificationDtos;
using Event_Parking_Reservation_System.Interfaces;
using static Event_Parking_Reservation_System.Exceptions.NotificationExceptions;
using static Event_Parking_Reservation_System.Interfaces.INotificationService;
using static Event_Parking_Reservation_System.Models.notification;

namespace Event_Parking_Reservation_System.Services
{
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly INotificationRepository _repository;

        public NotificationPublisher(INotificationRepository repository) =>
            _repository = repository;

        public async Task<Notification> CreateAsync(int customerId, NotificationType type, string message)
        {
            var notification = Notification.Create(customerId, type, message);
            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();
            return notification;
        }
    }

    public class NotificationQueryService : INotificationQueryService
    {
        private readonly INotificationRepository _repository;

        public NotificationQueryService(INotificationRepository repository) =>
            _repository = repository;

        public async Task<IReadOnlyList<NotificationDto>> GetForCustomerAsync(int customerId, int requestingCustomerId)
        {
            if (customerId != requestingCustomerId)
                throw new NotificationAccessDeniedException("You can only view your own notifications.");

            var items = await _repository.GetByCustomerIdAsync(customerId);
            return items
                .OrderByDescending(n => n.CreatedAt)
                .Select(NotificationDto.FromEntity)
                .ToList();
        }

        public async Task MarkAsReadAsync(int notificationId, int requestingCustomerId)
        {
            var notification = await _repository.GetByIdAsync(notificationId)
                ?? throw new NotificationNotFoundException(notificationId);

            if (notification.CustomerId != requestingCustomerId)
                throw new NotificationAccessDeniedException("You can only update your own notifications.");

            notification.MarkAsRead();
            await _repository.SaveChangesAsync();
        }
    }
}
