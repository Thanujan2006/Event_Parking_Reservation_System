using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using System.Text;

namespace Event_Parking_Reservation_System.Services
{
    public class ReceiptGenerator : IReceiptGenerator
    {
        public Task<ReceiptFile> GenerateAsync(Payment payment, int bookingId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("EVENT & PARKING RESERVATION SYSTEM — RECEIPT");
            sb.AppendLine("--------------------------------------------");
            sb.AppendLine($"Booking ID:   {bookingId}");
            sb.AppendLine($"Payment ID:   {payment.PaymentId}");
            sb.AppendLine($"Amount Paid:  {payment.Amount:C2}");
            sb.AppendLine($"Status:       {payment.Status}");
            sb.AppendLine($"Paid At:      {payment.PaidAt:u}");
            sb.AppendLine("--------------------------------------------");
            sb.AppendLine("This is a simulated payment — no real funds were transferred.");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return Task.FromResult(new ReceiptFile
            {
                Content = bytes,
                ContentType = "text/plain",
                FileName = $"receipt-booking-{bookingId}.txt"
            });
        }
    }
}
