using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Models;
using Microsoft.EntityFrameworkCore;
using Event_Parking_Reservation_System.Interfaces;


namespace Event_Parking_Reservation_System.Reposotires
{
    public class PaymentRepository:IPaymentRepository
    
    {
        private readonly AppDbContext _db;

        public PaymentRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Payment?> GetByBookingIdAsync(int bookingId) =>
            _db.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);

        public Task<Payment?> GetByIdAsync(int paymentId) =>
            _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        public async Task<IReadOnlyList<Payment>> GetByCustomerBookingIdsAsync(IEnumerable<int> bookingIds)
        {
            var ids = bookingIds.ToList();
            return await _db.Payments
                .Where(p => ids.Contains(p.BookingId))
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            // Row-locked re-check inside the transaction — protects against two concurrent
            // "Pay Now" clicks both succeeding (BRD Business Rule #2 / AC2).
            var alreadyExists = await _db.Payments
                .FromSqlInterpolated($"SELECT * FROM Payments WITH (UPDLOCK, ROWLOCK) WHERE BookingId = {payment.BookingId}")
                .AnyAsync();

            if (alreadyExists)
            {
                throw new DuplicatePaymentException();
            }

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return payment;
        }
    }
}
