namespace Event_Parking_Reservation_System.Exceptions
{
    
    
        public abstract class PaymentDomainException : Exception
        {
            public string ErrorCode { get; }

            protected PaymentDomainException(string errorCode, string message) : base(message)
            {
                ErrorCode = errorCode;
            }
        }

        /// <summary>404 — "Booking not found." (BRD 4.7.13).</summary>
        public class PaymentBookingNotFoundException : PaymentDomainException
        {
            public PaymentBookingNotFoundException(int bookingId)
                : base("BOOKING_NOT_FOUND", $"Booking {bookingId} not found.") { }
        }

        /// <summary>409 — "Payment already recorded for this booking." (BRD AC2).</summary>
        public class DuplicatePaymentException : PaymentDomainException
        {
            public DuplicatePaymentException()
                : base("PAYMENT_ALREADY_RECORDED", "Payment already recorded for this booking.") { }
        }

        /// <summary>400 — "This booking has expired and cannot be paid for." (BRD AC3).</summary>
        public class BookingExpiredForPaymentException : PaymentDomainException
        {
            public BookingExpiredForPaymentException()
                : base("BOOKING_EXPIRED", "This booking has expired and cannot be paid for.") { }
        }

        /// <summary>400 — booking exists but isn't in a payable state (e.g. already Confirmed/Cancelled).</summary>
        public class BookingNotPayableException : PaymentDomainException
        {
            public BookingNotPayableException(string currentStatus)
                : base("BOOKING_NOT_PAYABLE", $"Booking cannot be paid for while in '{currentStatus}' status.") { }
        }

        /// <summary>403 — payment/receipt access attempted by someone other than the owner or an admin.</summary>
        public class PaymentForbiddenException : PaymentDomainException
        {
            public PaymentForbiddenException()
                : base("PAYMENT_FORBIDDEN", "You are not authorized to perform this action.") { }
        }

        /// <summary>404 — receipt requested for a payment that doesn't exist.</summary>
        public class PaymentNotFoundException : PaymentDomainException
        {
            public PaymentNotFoundException(int paymentId)
                : base("PAYMENT_NOT_FOUND", $"Payment {paymentId} not found.") { }
        }
    }



