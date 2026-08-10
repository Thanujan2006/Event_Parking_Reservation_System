using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Enums;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_Parking_Reservation_System.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _db;

        public DashboardRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<int> GetTotalEventsAsync()
        {
            return await _db.Events.CountAsync();
        }
       

        public async Task<int> GetTotalBookingsAsync()
        {
            return await _db.Bookings.CountAsync();
        }

        public async Task<int> GetTotalAvailableSeatsAsync()
        {
            return await _db.Seats
                .CountAsync(s => s.Status == SeatStatus.Available);
        }

        public async Task<int> GetTotalOccupiedParkingSlotsAsync()
        {
            return await _db.ParkingSlots
                .CountAsync(p =>
                    p.Status == ParkingSlotStatus.Reserved ||
                    p.Status == ParkingSlotStatus.Held);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _db.Payments
                .Where(p => p.Status == paymentStatus.Completed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            return await _db.Customers.CountAsync();
        }
    }
}