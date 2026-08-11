

namespace Event_Parking_Reservation_System.DTOs
{
    /// <summary>
    /// POST /api/events/{eventId}/seats — admin seat map generation request.
    /// Rows * Columns must equal the event's Capacity (BRD 4.4.6 AC2).
    /// </summary>
    public class GenerateSeatMapRequest
    {
        public int Rows { get; set; }
        public int Columns { get; set; }

        /// <summary>Applied uniformly to every generated seat. Optional (BRD 4.4.4).</summary>
        public string? SeatType { get; set; }

        /// <summary>Optional price override applied uniformly. Null = use event base price.</summary>
        public decimal? PriceOverride { get; set; }
    }

    /// <summary>A single seat's state, as rendered in the visual seat map grid (BRD 4.4.2).</summary>
    public class SeatDto
    {
        public int SeatId { get; set; }
        public string RowLabel { get; set; } = string.Empty;
        public int ColumnNumber { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string? SeatType { get; set; }
        public decimal EffectivePrice { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>GET /api/events/{eventId}/seats response.</summary>
    public class SeatMapDto
    {
        public int EventId { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public IReadOnlyList<SeatDto> Seats { get; set; } = System.Array.Empty<SeatDto>();
    }

    /// <summary>
    /// POST /api/bookings/{bookingId}/seats — customer seat selection/hold request.
    /// At least one seat is required (BRD 4.4.12).
    /// </summary>
    public class HoldSeatsRequest
    {
        public List<int> SeatIds { get; set; } = new();
    }

    public class HoldSeatsResult
    {
        public bool Success { get; set; }
        public IReadOnlyList<int> HeldSeatIds { get; set; } = System.Array.Empty<int>();
        public IReadOnlyList<int> UnavailableSeatIds { get; set; } = System.Array.Empty<int>();
        public decimal RunningTotal { get; set; }
    }
}
