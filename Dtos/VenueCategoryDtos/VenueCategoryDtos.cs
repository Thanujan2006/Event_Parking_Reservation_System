
using System;
using System.Collections.Generic;

namespace Event_Parking_Reservation_System.Dtos
{
    // ---------- Venue DTOs ----------

    /// <summary>
    /// POST /api/venues — create request.
    /// </summary>
    public class CreateVenueRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int TotalCapacity { get; set; }
    }


    /// <summary>
    /// PUT /api/venues/{id} — update request.
    /// </summary>
    public class UpdateVenueRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int TotalCapacity { get; set; }
    }


    /// <summary>
    /// Venue dropdown/list item.
    /// Used by Event Management.
    /// </summary>
    public class VenueSummaryDto
    {
        public int VenueId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int TotalCapacity { get; set; }
    }


    /// <summary>
    /// Full venue detail view.
    /// </summary>
    public class VenueDetailDto
    {
        public int VenueId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int TotalCapacity { get; set; }

        public DateTime CreatedAt { get; set; }
    }


    /// <summary>
    /// GET /api/venues/available?startTime=&endTime=
    /// </summary>
    public class VenueAvailabilityQuery
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }


    // ---------- Category DTOs ----------

    /// <summary>
    /// POST /api/categories — create category request.
    /// </summary>
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
    }


    /// <summary>
    /// PUT /api/categories/{id} — update category request.
    /// </summary>
    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
    }


    /// <summary>
    /// Category response DTO.
    /// </summary>
    public class CategoryDto
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;
    }


    /// <summary>
    /// Generic paginated result.
    /// </summary>
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; }
            = Array.Empty<T>();

        public int TotalCount { get; set; }
    }
}
