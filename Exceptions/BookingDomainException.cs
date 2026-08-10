namespace Event_Parking_Reservation_System.Exceptions
{
    public class BookingDomainException : Exception
    {
        public string ErrorCode { get; }

        protected BookingDomainException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>400 — "A booking must contain at least one seat." (BRD Business Rule #1, AC1).</summary>
    public class EmptySeatListException : BookingDomainException
    {
        public EmptySeatListException()
            : base("EMPTY_SEAT_LIST", "A booking must contain at least one seat.") { }
    }

    /// <summary>404 — Booking record not found.</summary>
    public class BookingNotFoundException : BookingDomainException
    {
        public BookingNotFoundException(int bookingId)
            : base("BOOKING_NOT_FOUND", $"Booking {bookingId} not found.") { }
    }

    /// <summary>403 — Cancel/read attempted by someone other than the owning customer or an admin.</summary>
    public class BookingForbiddenException : BookingDomainException
    {
        public BookingForbiddenException()
            : base("BOOKING_FORBIDDEN", "You are not authorized to perform this action.") { }
    }

    /// <summary>400 — "This booking has expired. Please create a new booking." (BRD AC re: expired payment attempt).</summary>
    public class BookingExpiredException : BookingDomainException
    {
        public BookingExpiredException()
            : base("BOOKING_EXPIRED", "This booking has expired. Please create a new booking.") { }
    }

    /// <summary>409 — A requested seat is already Held/Booked by another active booking.</summary>
    public class SeatAlreadyBookedException : BookingDomainException
    {
        public SeatAlreadyBookedException(int seatId)
            : base("SEAT_ALREADY_BOOKED", $"Seat {seatId} was just booked by another customer.") { }
    }

    /// <summary>409 — The requested parking slot is already Held/Reserved.</summary>
    public class ParkingSlotAlreadyReservedException : BookingDomainException
    {
        public ParkingSlotAlreadyReservedException(int slotId)
            : base("PARKING_SLOT_ALREADY_RESERVED", $"Parking slot {slotId} was just reserved by another customer.") { }
    }
}

