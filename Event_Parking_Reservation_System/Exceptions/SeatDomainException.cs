using System;
using System.Collections.Generic;

namespace Event_Parking_Reservation_System.Exceptions
{
    public abstract class SeatDomainException : Exception
    {
        protected SeatDomainException(string message) : base(message) { }
    }

    public class EventNotFoundException : SeatDomainException
    {
        public EventNotFoundException(int eventId) : base($"Event not found: {eventId}") { }
    }

    public class SeatNotFoundException : SeatDomainException
    {
        public SeatNotFoundException(int seatId) : base($"Seat not found: {seatId}") { }
    }

    /// <summary>
    /// Thrown when Rows * Columns does not equal the event's Capacity
    /// -> maps to 400 Bad Request; seat map is not saved (BRD 4.4.6 AC2).
    /// </summary>
    public class SeatCountMismatchException : SeatDomainException
    {
        public SeatCountMismatchException(int seatMapCount, int eventCapacity)
            : base($"Seat map count ({seatMapCount}) does not equal event capacity ({eventCapacity}).") { }
    }

    /// <summary>Thrown when a seat map already exists for the event, to prevent accidental overwrite/duplication.</summary>
    public class SeatMapAlreadyExistsException : SeatDomainException
    {
        public SeatMapAlreadyExistsException(int eventId)
            : base($"A seat map already exists for event {eventId}.") { }
    }

    /// <summary>
    /// Thrown when one or more requested seats could not be held because they
    /// are no longer Available -> maps to 409 Conflict 'Seat already booked' (BRD 4.4.6 AC1).
    /// </summary>
    public class SeatsUnavailableException : SeatDomainException
    {
        public IReadOnlyList<int> UnavailableSeatIds { get; }

        public SeatsUnavailableException(IReadOnlyList<int> unavailableSeatIds)
            : base("One or more selected seats are no longer available.")
        {
            UnavailableSeatIds = unavailableSeatIds;
        }
    }

    /// <summary>Thrown when deleting/editing a seat that has an active (Held or Booked) booking -> maps to 409 Conflict (AC3).</summary>
    public class SeatHasActiveBookingException : SeatDomainException
    {
        public SeatHasActiveBookingException(int seatId)
            : base($"Seat {seatId} has an active booking and cannot be modified.") { }
    }

    /// <summary>Thrown on an invalid status transition (only Available->Held->Booked, or Held->Available, are valid).</summary>
    public class InvalidSeatStatusTransitionException : SeatDomainException
    {
        public InvalidSeatStatusTransitionException(string from, string to)
            : base($"Invalid seat status transition: {from} -> {to}.") { }
    }
}
