using Event_Parking_Reservation_System.Dtos.PaymentDtos;
using Event_Parking_Reservation_System.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Parking_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>GET /api/bookings/{id}/payment — amount due + payment status.</summary>
        [HttpGet("api/bookings/{id:int}/payment")]
        public async Task<ActionResult<AmountDueResponse>> GetAmountDue(int id)
        {
            var result = await _paymentService.GetAmountDueAsync(id, CurrentCustomerId(), IsAdmin());
            return Ok(result);
        }

        /// <summary>POST /api/bookings/{id}/payment — mark payment complete (simulation), confirm booking.</summary>
        [HttpPost("api/bookings/{id:int}/payment")]
        public async Task<ActionResult<PaymentResponse>> Pay(int id)
        {
            var result = await _paymentService.CompletePaymentAsync(id, CurrentCustomerId(), IsAdmin());
            return Ok(result);
        }

        /// <summary>GET /api/payments/customer/{customerId} — own payment history.</summary>
        [HttpGet("api/payments/customer/{customerId:int}")]
        public async Task<ActionResult<System.Collections.Generic.IReadOnlyList<PaymentHistoryDtos>>> GetHistory(int customerId)
        {
            if (!IsAdmin() && CurrentCustomerId() != customerId)
            {
                return Forbid();
            }

            return Ok(await _paymentService.GetCustomerHistoryAsync(customerId));
        }

        /// <summary>GET /api/payments/{id}/receipt — receipt download.</summary>
        [HttpGet("api/payments/{id:int}/receipt")]
        public async Task<IActionResult> GetReceipt(int id)
        {
            var file = await _paymentService.GetReceiptAsync(id, CurrentCustomerId(), IsAdmin());
            return File(file.Content, file.ContentType, file.FileName);
        }

        private int CurrentCustomerId() =>
            int.Parse(User.FindFirstValue("customerId") ?? "0");

        private bool IsAdmin() =>
            User.IsInRole("Admin");
    }
}

    

