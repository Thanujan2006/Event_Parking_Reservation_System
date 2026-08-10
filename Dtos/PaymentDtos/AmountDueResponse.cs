namespace Event_Parking_Reservation_System.Dtos.PaymentDtos
{
    public class AmountDueResponse
    {
        public int BookingId { get; set; }

        public decimal SeatsTotal { get; set; }

        public decimal ParkingFee { get; set; }

        public decimal AmountDue => SeatsTotal + ParkingFee;

        /// <summary>True if a Payment record already exists for this booking.</summary>
        public bool AlreadyPaid { get; set; }

        public string BookingStatus { get; set; } = string.Empty;
    }
}
