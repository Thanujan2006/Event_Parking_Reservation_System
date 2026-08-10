namespace Event_Parking_Reservation_System.Exceptions
{
    public class AuthExceptions
    {
        /// <summary>BRD 4.9.13 — expired or already-used token → 400.</summary>
        public class InvalidOrExpiredTokenException : Exception
        {
            public InvalidOrExpiredTokenException()
                : base("This link is invalid, has expired, or has already been used.") { }
        }

        /// <summary>BRD 4.9.13 — resend requested for an already-verified customer → 400.</summary>
        public class AlreadyVerifiedException : Exception
        {
            public AlreadyVerifiedException()
                : base("Your email is already verified.") { }
        }

        /// <summary>BRD Rule #1 — unverified customer attempted login/booking → 403.</summary>
        public class EmailNotVerifiedException : Exception
        {
            public EmailNotVerifiedException()
                : base("Please verify your email before continuing.") { }
        }

        /// <summary>
        /// Thrown internally when a customer isn't found. The controller
        /// deliberately never lets this reach the client on the
        /// forgot-password path (BRD Rule #8) — see AuthService for how it's
        /// swallowed into the generic response.
        /// </summary>
        public class CustomerNotFoundException : Exception
        {
            public CustomerNotFoundException() : base("Customer not found.") { }
        }

        /// <summary>Invalid email/password on login → 401 (generic message, no account enumeration).</summary>
        public class InvalidCredentialsException : Exception
        {
            public InvalidCredentialsException()
                : base("Invalid email or password.") { }
        }
    }
}
