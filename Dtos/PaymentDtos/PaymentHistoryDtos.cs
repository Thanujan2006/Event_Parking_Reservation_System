namespace Event_Parking_Reservation_System.Dtos.PaymentDtos
{
    public class PaymentHistoryDtos
    {
        public int PaymentId { get; set; }

        public int BookingId { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime PaidAt { get; set; }
    }
}
