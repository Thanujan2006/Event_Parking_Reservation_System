using System.ComponentModel.DataAnnotations;

using Event_Parking_Reservation_System.Models;
using Event_Parking_Reservation_System.Models;
using System.ComponentModel.DataAnnotations;
using Amazon.SimpleSystemsManagement;
using static Event_Parking_Reservation_System.Models.notification;

namespace Event_Parking_Reservation_System.Dtos.NotificationDtos
{
    /// <summary>
    /// Outbound shape for a notification.
    /// </summary>
    public class NotificationDto
    {
        public int Id { get; set; }   // Changed to match entity
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public static NotificationDto FromEntity(Notification n)
        {
            return new NotificationDto
            {
                Id = n.Id,   // Use entity property name
                Type = n.Type.ToString() ?? string.Empty,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            };
        }
    }

    /// <summary>
    /// Inbound shape for POST /api/notifications.
    /// </summary>
    public class CreateNotificationRequest
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public Amazon.SimpleSystemsManagement.NotificationType Type { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Message must not exceed 500 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}


