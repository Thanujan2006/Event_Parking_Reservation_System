using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Enums;
using Event_Parking_Reservation_System.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Event_Parking_Reservation_System.Interfaces.IExternalgateways;
using static Event_Parking_Reservation_System.Interfaces.INotificationService;
using static Event_Parking_Reservation_System.Models.notification;

namespace Event_Parking_Reservation_System.Services
{
    public class BookingGateway : IBookingGateway
    {
        private readonly AppDbContext _db;

        public BookingGateway(AppDbContext db) => _db = db;

        public async Task<BookingSnapshot?> GetBookingAsync(int bookingId)
        {
            var booking = await _db.Bookings.AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            return booking is null
                ? null
                : new BookingSnapshot
                {
                    BookingId = booking.BookingId,
                    CustomerId = booking.CustomerId,
                    EventId = booking.EventId,
                    Status = booking.Status.ToString()
                };
        }

        public async Task ConfirmBookingAsync(int bookingId)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId)
                ?? throw new InvalidOperationException($"Booking {bookingId} not found.");

            booking.Status = BookingStatus.Confirmed;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<int>> ListBookingIdsByCustomerAsync(int customerId) =>
            await _db.Bookings.AsNoTracking()
                .Where(b => b.CustomerId == customerId)
                .Select(b => b.BookingId)
                .ToListAsync();
    }

    public class SeatPricingGateway : ISeatPricingGateway
    {
        private readonly AppDbContext _db;

        public SeatPricingGateway(AppDbContext db) => _db = db;

        public async Task<decimal> GetSeatsTotalAsync(int bookingId)
        {
            var booking = await _db.Bookings.AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking is null) return 0m;

            var eventPrice = await _db.Events.AsNoTracking()
                .Where(e => e.EventId == booking.EventId)
                .Select(e => e.TicketPrice)
                .FirstOrDefaultAsync();

            var seatIds = await _db.BookingSeats.AsNoTracking()
                .Where(bs => bs.BookingId == bookingId)
                .Select(bs => bs.SeatId)
                .ToListAsync();

            if (seatIds.Count == 0) return 0m;

            var seats = await _db.Seats.AsNoTracking()
                .Where(s => seatIds.Contains(s.SeatId))
                .ToListAsync();

            return seats.Sum(s => s.PriceOverride ?? eventPrice);
        }
    }

    public class ParkingFeeGateway : IParkingFeeGateway
    {
        private readonly AppDbContext _db;

        public ParkingFeeGateway(AppDbContext db) => _db = db;

        public async Task<decimal> GetParkingFeeAsync(int bookingId)
        {
            var reservation = await _db.ParkingReservations.AsNoTracking()
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);

            return reservation?.FeeAtReservation ?? 0m;
        }
    }

    public class PaymentNotificationGateway : IPaymentNotificationGateway
    {
        private readonly INotificationPublisher _publisher;

        public PaymentNotificationGateway(INotificationPublisher publisher) =>
            _publisher = publisher;

        public async Task NotifyPaymentConfirmedAsync(int customerId, int bookingId, decimal amount)
        {
            await _publisher.CreateAsync(
                customerId,
                NotificationType.Confirmed,
                $"Payment of {amount:C2} confirmed for booking #{bookingId}.");
        }
    }
}
