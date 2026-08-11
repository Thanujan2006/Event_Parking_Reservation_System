using System;

namespace Event_Parking_Reservation_System.Exceptions
{
    // NOTE: VenueNotFoundException and CategoryNotFoundException are already
    // defined in VenueCategoryExceptions.cs within this same
    // Event_Parking_Reservation_System.Exceptions namespace. Reused here —
    // not redefined, to avoid CS0101 duplicate-definition error.

    /// <summary>Thrown when an Event is not found -> maps to 404 Not Found.</summary>
    public class EventNotFoundException : VenueCategoryDomainException
    {
        public EventNotFoundException(int eventId)
            : base($"Event not found: {eventId}")
        {
        }
    }

    /// <summary>
    /// Thrown when the new/updated event's time overlaps another event already
    /// scheduled at the same venue -> maps to 409 Conflict (BRD 4.3.6 AC1).
    /// </summary>
    public class VenueScheduleOverlapException : VenueCategoryDomainException
    {
        public VenueScheduleOverlapException(int venueId)
            : base($"Venue {venueId} already has an event scheduled that overlaps this time.")
        {
        }
    }

    /// <summary>
    /// Thrown when Event Capacity exceeds the parent Venue's TotalCapacity
    /// -> maps to 400 Bad Request (BRD 4.3.6 AC2).
    /// </summary>
    public class EventCapacityExceedsVenueException : VenueCategoryDomainException
    {
        public EventCapacityExceedsVenueException(int eventCapacity, int venueCapacity)
            : base($"Event capacity ({eventCapacity}) exceeds venue capacity ({venueCapacity}).")
        {
        }
    }
}