
using System;

namespace Event_Parking_Reservation_System.Exceptions
{
    public abstract class VenueCategoryDomainException : Exception
    {
        protected VenueCategoryDomainException(string message)
            : base(message)
        {
        }
    }


    /// <summary>
    /// Thrown when a Venue is not found.
    /// Maps to 404 Not Found.
    /// </summary>
    public class VenueNotFoundException : VenueCategoryDomainException
    {
        public VenueNotFoundException(int venueId)
            : base($"Venue not found: {venueId}")
        {
        }
    }


    /// <summary>
    /// Thrown when a Category is not found.
    /// Maps to 404 Not Found.
    /// </summary>
    public class CategoryNotFoundException : VenueCategoryDomainException
    {
        public CategoryNotFoundException(int categoryId)
            : base($"Category not found: {categoryId}")
        {
        }
    }


    /// <summary>
    /// Thrown when deleting a Venue that has upcoming
    /// events scheduled.
    /// Maps to 409 Conflict.
    /// </summary>
    public class VenueHasUpcomingEventsException
        : VenueCategoryDomainException
    {
        public VenueHasUpcomingEventsException(int venueId)
            : base(
                $"Venue {venueId} has upcoming scheduled events and cannot be deleted.")
        {
        }
    }


    /// <summary>
    /// Thrown when deleting a Category that is still
    /// referenced by events.
    /// Maps to 409 Conflict.
    /// </summary>
    public class CategoryInUseException : VenueCategoryDomainException
    {
        public CategoryInUseException(int categoryId)
            : base(
                $"Category {categoryId} is in use by one or more events and cannot be deleted.")
        {
        }
    }


    /// <summary>
    /// Thrown when a Category name already exists.
    /// Maps to 409 Conflict.
    /// </summary>
    public class DuplicateCategoryNameException
        : VenueCategoryDomainException
    {
        public DuplicateCategoryNameException(string name)
            : base($"Category name already exists: {name}")
        {
        }
    }


    /// <summary>
    /// Thrown when startTime is greater than or equal to endTime.
    /// </summary>
    public class InvalidTimeRangeException
        : VenueCategoryDomainException
    {
        public InvalidTimeRangeException()
            : base("startTime must be earlier than endTime.")
        {
        }
    }
}

 