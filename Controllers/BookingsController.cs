using Event_Parking_Reservation_System.Dtos.BookingDtos;
using Event_Parking_Reservation_System.Services;
using Event_Parking_Reservation_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Event_Parking_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>POST /api/bookings — create booking + start hold (BRD AC1, AC2).</summary>
        [HttpPost]
        public async Task<ActionResult<CreateBookingResponse>> Create([FromBody] CreateBookingRequest request)
        {
            var result = await _bookingService.CreateBookingAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.BookingId }, result);
        }

        /// <summary>GET /api/bookings/customer/{customerId} — own booking history.</summary>
        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<System.Collections.Generic.IReadOnlyList<BookingDtos>>> GetCustomerHistory(int customerId)
        {
            if (!IsAdmin() && CurrentCustomerId() != customerId)
            {
                return Forbid();
            }

            return Ok(await _bookingService.GetCustomerHistoryAsync(customerId));
        }

        /// <summary>GET /api/bookings/{id} — full booking detail (owner or admin).</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingDtos>> GetById(int id)
        {
            var dto = await _bookingService.GetByIdAsync(id, CurrentCustomerId(), IsAdmin());
            return Ok(dto);
        }

        /// <summary>GET /api/bookings/{id}/hold-status — countdown timer data (BRD 4.6.14 UI Flow).</summary>
        [HttpGet("{id:int}/hold-status")]
        public async Task<ActionResult<HoldStatusResponse>> GetHoldStatus(int id)
        {
            return Ok(await _bookingService.GetHoldStatusAsync(id));
        }

        /// <summary>DELETE /api/bookings/{id} — cancel + release seats/parking (BRD AC4).</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel(int id)
        {
            await _bookingService.CancelAsync(id, CurrentCustomerId(), IsAdmin());
            return Ok();
        }

        /// <summary>GET /api/bookings?eventId= — admin-only, event-wide booking list.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<System.Collections.Generic.IReadOnlyList<BookingDtos>>> GetByEvent([FromQuery] int eventId)
        {
            return Ok(await _bookingService.GetByEventIdAsync(eventId));
        }

        private int CurrentCustomerId() =>
            int.Parse(User.FindFirstValue("customerId") ?? "0");

        private bool IsAdmin() =>
            User.IsInRole("Admin");
    }
}

    

