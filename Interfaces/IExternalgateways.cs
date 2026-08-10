namespace Event_Parking_Reservation_System.Interfaces
{
    public interface IExternalgateways
    {
        public class BookingSnapshot
        {
            public int BookingId { get; set; }
            public int CustomerId { get; set; }
            public int EventId { get; set; }
            public string Status { get; set; } = string.Empty; // Pending | Confirmed | Cancelled | Expired
        }

        /// <summary>
        /// Port into Module 6 (Booking Management) — implemented there. This module only
        /// reads booking state and asks it to confirm; it never touches the Bookings table
        /// directly, keeping module ownership boundaries intact (BRD 7.4).
        /// </summary>
        public interface IBookingGateway
        {
            Task<BookingSnapshot?> GetBookingAsync(int bookingId);

            /// <summary>Flips Pending -> Confirmed and clears the hold (BRD Business Rule #1).</summary>
            Task ConfirmBookingAsync(int bookingId);

            /// <summary>All booking IDs belonging to a customer — backs GET /api/payments/customer/{id}.</summary>
            Task<System.Collections.Generic.IReadOnlyList<int>> ListBookingIdsByCustomerAsync(int customerId);
        }

        /// <summary>Port into Modules 3/4 — sums ticket price across a booking's seats.</summary>
        public interface ISeatPricingGateway
        {
            Task<decimal> GetSeatsTotalAsync(int bookingId);
        }

        /// <summary>Port into Module 5 — returns the fee snapshot if a parking slot was reserved.</summary>
        public interface IParkingFeeGateway
        {
            Task<decimal> GetParkingFeeAsync(int bookingId);
        }

        /// <summary>Port into Module 8 — fires the "payment confirmed" notification (BRD AC1).</summary>
        public interface IPaymentNotificationGateway
        {
            Task NotifyPaymentConfirmedAsync(int customerId, int bookingId, decimal amount);
        }
    }

}

