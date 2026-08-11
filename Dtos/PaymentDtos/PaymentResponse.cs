namespace Event_Parking_Reservation_System.Dtos.PaymentDtos
{
    public class PaymentResponse
    {
        public int BookingId { get; set; }

        public decimal AmountPaid { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime PaidAt { get; set; }
    }
}
