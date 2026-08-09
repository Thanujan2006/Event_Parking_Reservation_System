namespace Event_Parking_Reservation_System.Exceptions
{
    public class NotificationExceptions
    {
        /// <summary>BRD 4.8.13 — Invalid NotificationId Read Attempt → 404.</summary>
        public class NotificationNotFoundException : Exception
        {
            public NotificationNotFoundException(int notificationId)
                : base($"Notification {notificationId} not found.") { }
        }

        /// <summary>
        /// BRD 4.8.13 — a customer tried to access another customer's
        /// notifications, or a non-internal caller hit the create endpoint.
        /// Mapped to 403 Forbidden by the controller.
        /// </summary>
        public class NotificationAccessDeniedException : Exception
        {
            public NotificationAccessDeniedException(string message) : base(message) { }
        }



    }
}
