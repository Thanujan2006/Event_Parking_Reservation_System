namespace Event_Parking_Reservation_System.Models
{
    public class notification
    {

        /// <summary>
        /// Types of notifications the system can raise.
        /// BRD 4.8.10 — Table: Notifications.Type
        /// </summary>
        public enum NotificationType
        {
            Confirmed,
            Cancelled,
            Reminder,
            EventUpdate
        }

        /// <summary>
        /// A single in-app, stored notification for a customer.
        /// BRD 4.8.10 — Table: Notifications
        /// Real-time delivery (SignalR/Push) is explicitly out of scope (BRD 4.8.3).
        /// </summary>
        public class Notification
        {
            public int NotificationId { get; private set; }
            public int CustomerId { get; private set; }
            public NotificationType Type { get; private set; }
            public string Message { get; private set; } = string.Empty;
            public bool IsRead { get; private set; }
            public DateTime CreatedAt { get; private set; }
            public int Id { get; internal set; }

            // EF Core requires a parameterless constructor.
            private Notification() { }

            /// <summary>
            /// Factory method — this is the only way to create a Notification,
            /// which keeps creation rules (BRD Rule #2: internal-only, never
            /// created directly by a user) enforced at the domain level too,
            /// not just at the controller/authorization layer.
            /// </summary>
            public static Notification Create(int customerId, NotificationType type, string message)
            {
                if (customerId <= 0)
                    throw new ArgumentException("CustomerId must be a valid, positive identifier.", nameof(customerId));

                if (string.IsNullOrWhiteSpace(message))
                    throw new ArgumentException("Message is required.", nameof(message));

                if (message.Length > 500)
                    throw new ArgumentException("Message must not exceed 500 characters.", nameof(message));

                return new Notification
                {
                    CustomerId = customerId,
                    Type = type,
                    Message = message.Trim(),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
            }

            /// <summary>BRD 4.8.11 — PUT /api/notifications/{id}/read</summary>
            public void MarkAsRead()
            {
                IsRead = true;
            }


        }
    }
}
