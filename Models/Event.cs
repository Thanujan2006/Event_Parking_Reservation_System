using System;

namespace Event_Parking_Reservation_System.Models
{
    /// <summary>
    /// Domain entity representing a bookable event.
    /// Maps to the "Events" table (see Database/03_Events.sql).
    /// BRD 4.3.4 — single date/time slot only (no multi-session events);
    /// TicketPrice is a flat rate (seat-type pricing is optional, handled in Module 4).
    /// </summary>
    public class Event
    {
        public int EventId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int VenueId { get; set; }

        public int CategoryId { get; set; }

        public DateTime EventDateTime { get; set; }

        /// <summary>Event duration, used for overlap checks against the venue's schedule.</summary>
        public int DurationMinutes { get; set; }

        public decimal TicketPrice { get; set; }

        /// <summary>Must not exceed the parent Venue's TotalCapacity (BRD 4.3.6 AC2).</summary>
        public int Capacity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}