using Event_Parking_Reservation_System.Models;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface IReceiptGenerator
    {
        Task<ReceiptFile> GenerateAsync(Payment payment, int bookingId);
    }
}

