using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Parking_Reservation_System.Models
{
    /// <summary>
    /// Table: ParkingReservations (BRD 4.5.10)
    /// Links a Booking to the single ParkingSlot it reserved (optional per booking).
    /// FeeAtReservation is an immutable snapshot — BRD Business Rule #5:
    /// "Slot Fee is fixed at the moment of reservation and never changes retroactively."
    /// </summary>
    public class ParkingReservation
    {
        [Key]
        public int ReservationId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int SlotId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal FeeAtReservation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(BookingId))]
        public virtual Booking? Booking { get; set; }

        [ForeignKey(nameof(SlotId))]
        public virtual ParkingSlot? ParkingSlot { get; set; }
    }
}
