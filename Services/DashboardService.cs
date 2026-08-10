using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Enums;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;
using static Event_Parking_Reservation_System.Dtos.DashBoardDtos.DashboardDtos;

namespace Event_Parking_Reservation_System.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService.INotificationQueryService _notificationQueryService;

        public DashboardService(AppDbContext db, IBookingService bookingService,
            IPaymentService paymentService, INotificationService.INotificationQueryService notificationQueryService)
        {
            _db = db;
            _bookingService = bookingService;
            _paymentService = paymentService;
            _notificationQueryService = notificationQueryService;
        }

        public async Task<CustomerDashboardResponse> GetCustomerDashboardAsync(int customerId)
        {
            var allBookings = await _bookingService.GetCustomerHistoryAsync(customerId);

            var upcoming = allBookings
                .Where(b => b.Status is "Pending" or "Confirmed")
                .OrderBy(b => b.HoldExpiresAt ?? DateTime.MaxValue)
                .ToList();

            var reservedParking = upcoming
                .Where(b => b.ParkingSlotId is not null)
                .Select(b => b.ParkingSlotId!)
                .ToList();

            var recentPayments = (await _paymentService.GetCustomerHistoryAsync(customerId))
                .Take(5)
                .ToList();

            var notifications = await _notificationQueryService.GetForCustomerAsync(customerId, customerId);
            var unread = notifications.Where(n => !n.IsRead).ToList();

            return new CustomerDashboardResponse(upcoming, reservedParking, recentPayments, unread.Count, unread);
        }
      

        public async Task<AdminDashboardResponse> GetAdminDashboardAsync()
        {
            var totalEvents = await _db.Events.CountAsync();
            var totalBookings = await _db.Bookings.CountAsync();
            var totalAvailableSeats = await _db.Seats.CountAsync(s => s.Status == SeatStatus.Available);
            var totalOccupiedParkingSlots = await _db.ParkingSlots.CountAsync(p =>
                p.Status == ParkingSlotStatus.Reserved || p.Status == ParkingSlotStatus.Held);
            var totalRevenue = await _db.Payments
                .Where(p => p.Status == paymentStatus.Completed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var totalCustomers = await _db.Customers.CountAsync(c => c.Role == "Customer");

            return new AdminDashboardResponse(
                totalEvents, totalBookings, totalAvailableSeats, totalOccupiedParkingSlots, totalRevenue, totalCustomers);
        }


    }
}
