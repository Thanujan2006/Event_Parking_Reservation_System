using System.ComponentModel.DataAnnotations;
using static Event_Parking_Reservation_System.Models.notification;

namespace Event_Parking_Reservation_System.Dtos.NotificationDtos
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public static NotificationDto FromEntity(Notification n) => new()
        {
            Id = n.NotificationId,
            Type = n.Type.ToString(),
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }

    public class CreateNotificationRequest
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Message must not exceed 500 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}
