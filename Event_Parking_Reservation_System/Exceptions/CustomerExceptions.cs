namespace Event_Parking_Reservation_System.Exceptions
{
    public class CustomerExceptions
    {
        /// <summary>Base type for all Customer-module domain exceptions.</summary>
        public abstract class CustomerDomainException : Exception
        {
            protected CustomerDomainException(string message) : base(message) { }
        }

        /// <summary>Thrown on duplicate registration email -> maps to 409 Conflict (AC2).</summary>
        public class DuplicateEmailException : CustomerDomainException
        {
            public DuplicateEmailException(string email)
                : base($"Email already registered: {email}") { }
        }

        /// <summary>Thrown when a customer is not found -> maps to 404 Not Found.</summary>
        public class CustomerNotFoundException : CustomerDomainException
        {
            public CustomerNotFoundException(int customerId)
                : base($"Customer not found: {customerId}") { }
        }

        /// <summary>
        /// Thrown when trying to deactivate a customer with active future bookings
        /// -> maps to 400 Bad Request, includes booking references (AC3).
        /// </summary>
        public class ActiveBookingsExistException : CustomerDomainException
        {
            public int[] BookingIds { get; }

            public ActiveBookingsExistException(int[] bookingIds)
                : base("Customer has active future bookings and cannot be deactivated.")
            {
                BookingIds = bookingIds;
            }
        }

        /// <summary>Thrown when a deactivated customer attempts to log in -> maps to 403 Forbidden (AC4).</summary>
        public class AccountDeactivatedException : CustomerDomainException
        {
            public AccountDeactivatedException()
                : base("Account Deactivated") { }
        }

        /// <summary>Thrown when caller attempts to change the Email field on profile update (BRD 4.1.12).</summary>
        public class EmailChangeNotAllowedException : CustomerDomainException
        {
            public EmailChangeNotAllowedException()
                : base("Email cannot be changed via profile update.") { }
        }
    }
}
