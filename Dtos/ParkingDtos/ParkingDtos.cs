using System.ComponentModel.DataAnnotations;

namespace Event_Parking_Reservation_System.DTOs
{
    /// <summary>Response shape for a single slot in the layout (BRD 4.5.11 GET response).</summary>
    public class ParkingSlotDto
    {
        public int SlotId { get; set; }
        public int EventId { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Fee { get; set; }
    }

    /// <summary>One slot entry when Admin builds/extends a layout.</summary>
    public class ParkingSlotCreateItem
    {
        [Required]
        [MaxLength(20)]
        public string SlotNumber { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Fee must be non-negative.")]
        public decimal Fee { get; set; }
    }

    /// <summary>Request body for POST /api/events/{eventId}/parking-slots (BRD 4.5.11).</summary>
    public class ParkingLayoutCreateDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one slot is required.")]
        public List<ParkingSlotCreateItem> Slots { get; set; } = new();
    }

    /// <summary>Request body for PUT /api/events/{eventId}/parking-slots/{slotId}.</summary>
    public class ParkingSlotUpdateDto
    {
        [MaxLength(20)]
        public string? SlotNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Fee must be non-negative.")]
        public decimal? Fee { get; set; }
    }

    /// <summary>Request body for POST /api/bookings/{bookingId}/parking (BRD 4.5.11).</summary>
    public class ParkingReserveDto
    {
        [Required]
        public int SlotId { get; set; }
    }

    /// <summary>Response shape for a confirmed reservation.</summary>
    public class ParkingReservationDto
    {
        public int ReservationId { get; set; }
        public int BookingId { get; set; }
        public int SlotId { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public decimal FeeAtReservation { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Uniform error payload matching the BRD's Exception Handling table (4.5.13).</summary>
    public class ApiErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
