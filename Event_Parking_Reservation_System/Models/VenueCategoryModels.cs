using System;

namespace Event_Parking_Reservation_System.Models
{
    /// <summary>
    /// Domain entity representing a physical venue where events take place.
    /// Maps to the "Venues" table (see Database/01_Venues.sql).
    /// BRD 4.2 — a venue holds a single flat TotalCapacity (no zone-wise sub-capacity).
    /// </summary>
    public class Venue
    {
        public int VenueId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Free-text address (BRD 4.2.4 — no structured City/State/Zip yet).</summary>
        public string Address { get; set; } = string.Empty;

        public int TotalCapacity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }


    /// <summary>
    /// Domain entity representing an event category used for browsing/filtering.
    /// Maps to the "EventCategories" table. Manually curated static list (BRD 4.2.4).
    /// </summary>
    public class EventCategory
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}