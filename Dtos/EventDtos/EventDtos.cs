using System;

namespace Event_Parking_Reservation_System.Dtos
{
    /// <summary>POST /api/events — create request (BRD 4.3.2).</summary>
    public class CreateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public int VenueId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EventDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public decimal TicketPrice { get; set; }
        public int Capacity { get; set; }
    }

    /// <summary>PUT /api/events/{id} — update request.</summary>
    public class UpdateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public int VenueId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EventDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public decimal TicketPrice { get; set; }
        public int Capacity { get; set; }
    }

    /// <summary>Customer-facing browse/search result item (BRD 4.3.2).</summary>
    public class EventSummaryDto
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string VenueName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EventDateTime { get; set; }

        /// <summary>Date-only value used by the SPA cards (formatDate).</summary>
        public string EventDate { get; set; } = string.Empty;

        /// <summary>Start time "HH:mm:ss" used by the SPA (formatTime).</summary>
        public string StartTime { get; set; } = string.Empty;

        public decimal TicketPrice { get; set; }
        public int Capacity { get; set; }
        public int SeatsAvailable { get; set; }
    }

    /// <summary>Full detail view for admin edit screen / event detail page.</summary>
    public class EventDetailDto
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EventDateTime { get; set; }
        public string EventDate { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal TicketPrice { get; set; }
        public int Capacity { get; set; }
        public int SeatsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Customer-facing search/filter query params (BRD 4.3.2 — Name, Date, Venue, Category).</summary>
    public class EventSearchQuery
    {
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
        public int? VenueId { get; set; }
        public int? CategoryId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // NOTE: PagedResult<T> is already defined in VenueCategoryDtos.cs
    // within this same Event_Parking_Reservation_System.Dtos namespace.
    // Not redefined here to avoid CS0101 duplicate-definition error.
}