using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Event_Parking_Reservation_System.Enums;


namespace Event_Parking_Reservation_System.Models
{
    /// <summary>
    /// Table: ParkingSlots (BRD 4.5.10)
    /// One row per physical parking slot/zone belonging to a single Event.
    /// </summary>
    public class ParkingSlot
    {
        [Key]
        public int SlotId { get; set; }

        [Required]
        public int EventId { get; set; }

        /// <summary>Unique per Event (BRD 4.5.12).</summary>
        [Required]
        [MaxLength(20)]
        public string SlotNumber { get; set; } = string.Empty;

        [Required]
        public ParkingSlotStatus Status { get; set; } = ParkingSlotStatus.Available;

        /// <summary>
        /// Fee configured by Admin for this slot at layout-creation time.
        /// Note: actual charged fee is snapshotted onto ParkingReservation.FeeAtReservation
        /// and must never be read back from here after a reservation exists (BRD Rule #5).
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal Fee { get; set; }

        [ForeignKey(nameof(EventId))]
        public virtual Event? Event { get; set; }

        public virtual ParkingReservation? Reservation { get; set; }
    }


    /// <summary>
    /// Minimal stub — the real Booking entity lives in Module 6 (Booking Management).
    /// </summary>
    public class BookingStub
    {
        public int BookingId { get; set; }
    }
}