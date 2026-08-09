using Event_Parking_Reservation_System.Dtos.PaymentDtos;
using Event_Parking_Reservation_System.Enums;
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using static Event_Parking_Reservation_System.Interfaces.IExternalgateways;

namespace Event_Parking_Reservation_System.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingGateway _bookingGateway;
        private readonly ISeatPricingGateway _seatPricingGateway;
        private readonly IParkingFeeGateway _parkingFeeGateway;
        private readonly IPaymentNotificationGateway _notificationGateway;
        private readonly IReceiptGenerator _receiptGenerator;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IBookingGateway bookingGateway,
            ISeatPricingGateway seatPricingGateway,
            IParkingFeeGateway parkingFeeGateway,
            IPaymentNotificationGateway notificationGateway,
            IReceiptGenerator receiptGenerator)
        {
            _paymentRepository = paymentRepository;
            _bookingGateway = bookingGateway;
            _seatPricingGateway = seatPricingGateway;
            _parkingFeeGateway = parkingFeeGateway;
            _notificationGateway = notificationGateway;
            _receiptGenerator = receiptGenerator;
        }

        public async Task<AmountDueResponse> GetAmountDueAsync(int bookingId, int requestingCustomerId, bool isAdmin)
        {
            var booking = await _bookingGateway.GetBookingAsync(bookingId)
                ?? throw new PaymentBookingNotFoundException(bookingId);

            if (!isAdmin && booking.CustomerId != requestingCustomerId)
            {
                throw new PaymentForbiddenException();
            }

            var seatsTotal = await _seatPricingGateway.GetSeatsTotalAsync(bookingId);
            var parkingFee = await _parkingFeeGateway.GetParkingFeeAsync(bookingId);
            var existingPayment = await _paymentRepository.GetByBookingIdAsync(bookingId);

            return new AmountDueResponse
            {
                BookingId = bookingId,
                SeatsTotal = seatsTotal,
                ParkingFee = parkingFee,
                AlreadyPaid = existingPayment != null,
                BookingStatus = booking.Status
            };
        }

        public async Task<PaymentResponse> CompletePaymentAsync(int bookingId, int requestingCustomerId, bool isAdmin)
        {
            var booking = await _bookingGateway.GetBookingAsync(bookingId)
                ?? throw new PaymentBookingNotFoundException(bookingId);

            if (!isAdmin && booking.CustomerId != requestingCustomerId)
            {
                throw new PaymentForbiddenException();
            }

            // BRD 4.7.12: "Booking Status — Pending & Not Expired மட்டும் Payment அனுமதி".
            if (string.Equals(booking.Status, "Expired", StringComparison.OrdinalIgnoreCase))
            {
                throw new BookingExpiredForPaymentException();
            }

            if (!string.Equals(booking.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                // Covers Confirmed (already paid via the duplicate check below in the normal
                // path, but also guards a Cancelled booking) — BRD AC3-adjacent guard.
                throw new BookingNotPayableException(booking.Status);
            }

            // BRD 4.7.12: never trust a client-supplied amount — always recompute.
            var seatsTotal = await _seatPricingGateway.GetSeatsTotalAsync(bookingId);
            var parkingFee = await _parkingFeeGateway.GetParkingFeeAsync(bookingId);
            var amount = seatsTotal + parkingFee;

            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                Status = paymentStatus.Completed,
                PaidAt = DateTime.UtcNow
            };

            // Throws DuplicatePaymentException under a row lock if one already exists —
            // covers the race between this status check and the insert (BRD Rule #2).
            var created = await _paymentRepository.CreateAsync(payment);

            // Only flip the booking to Confirmed after the payment record is durably saved
            // (BRD Business Rule #1: "Payment Complete ஆகும் வரை Booking Confirmed மாற முடியாது").
            await _bookingGateway.ConfirmBookingAsync(bookingId);

            await _notificationGateway.NotifyPaymentConfirmedAsync(booking.CustomerId, bookingId, amount);

            return new PaymentResponse
            {
                BookingId = bookingId,
                AmountPaid = created.Amount,
                Status = "Confirmed",
                PaidAt = created.PaidAt
            };
        }

        public async Task<IReadOnlyList<PaymentHistoryDtos>> GetCustomerHistoryAsync(int customerId)
        {
            var bookingIds = await _bookingGateway.ListBookingIdsByCustomerAsync(customerId);
            var payments = await _paymentRepository.GetByCustomerBookingIdsAsync(bookingIds);

            return payments.Select(p => new PaymentHistoryDtos
            {
                PaymentId = p.PaymentId,
                BookingId = p.BookingId,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                PaidAt = p.PaidAt
            }).ToList();
        }

        public async Task<ReceiptFile> GetReceiptAsync(int paymentId, int requestingCustomerId, bool isAdmin)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId)
                ?? throw new PaymentNotFoundException(paymentId);

            var booking = await _bookingGateway.GetBookingAsync(payment.BookingId);
            if (booking != null && !isAdmin && booking.CustomerId != requestingCustomerId)
            {
                throw new PaymentForbiddenException();
            }

            return await _receiptGenerator.GenerateAsync(payment, payment.BookingId);
        }
    }
}
