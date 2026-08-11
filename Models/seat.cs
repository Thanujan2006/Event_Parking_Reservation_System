using System;

namespace Event_Parking_Reservation_System.Models
{
    /// <summary>
    /// Domain entity representing a single seat within an event's seat map.
    /// Maps to the "Seats" table (see Database/04_Seats.sql).
    /// BRD 4.4.4 — rectangular grid only (Rows x Columns), no irregular shapes.
    /// </summary>
    public class Seat
    {
        public int SeatId { get; set; }

        public int EventId { get; set; }

        public string RowLabel { get; set; } = string.Empty;

        public int ColumnNumber { get; set; }

        /// <summary>Unique within the event, e.g. "A1" (BRD 4.4.12).</summary>
        public string SeatNumber { get; set; } = string.Empty;

        /// <summary>Optional price tier. If null, the event's base TicketPrice applies (BRD 4.4.4).</summary>
        public string? SeatType { get; set; }

        /// <summary>Optional per-seat price override tied to SeatType. Null = use event base price.</summary>
        public decimal? PriceOverride { get; set; }

        public SeatStatus Status { get; set; } = SeatStatus.Available;

        /// <summary>
        /// Set while the seat is Held or Booked, referencing the owning booking.
        /// Null while Available. FK enforced once the Booking module's table exists
        /// (see Database/04_Seats.sql notes).
        /// </summary>
        public int? BookingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Valid transitions per BRD 4.4.12: Available -> Held -> Booked only.
    /// Held seats can also revert to Available (hold expiry / cancellation).
    /// </summary>
    public enum SeatStatus
    {
        Available = 0,
        Held = 1,
        Booked = 2
    }
}
