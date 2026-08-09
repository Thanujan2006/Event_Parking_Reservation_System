using Event_Parking_Reservation_System.Dtos.PaymentDtos;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface IPaymentService
    {
        Task<AmountDueResponse> GetAmountDueAsync(int bookingId, int requestingCustomerId, bool isAdmin);

        Task<PaymentResponse> CompletePaymentAsync(int bookingId, int requestingCustomerId, bool isAdmin);

        Task<IReadOnlyList<PaymentHistoryDtos>> GetCustomerHistoryAsync(int customerId);

        Task<ReceiptFile> GetReceiptAsync(int paymentId, int requestingCustomerId, bool isAdmin);
    }
    public class ReceiptFile
    {
        public byte[] Content { get; set; } = System.Array.Empty<byte>();
        public string ContentType { get; set; } = "application/pdf";
        public string FileName { get; set; } = string.Empty;
    }
}


