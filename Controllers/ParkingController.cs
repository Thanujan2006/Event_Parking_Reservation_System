using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Event_Parking_Reservation_System.DTOs;
using Event_Parking_Reservation_System.Services;

namespace Event_Parking_Reservation_System.Controllers
{
    /// <summary>
    /// Implements the endpoint table in BRD 4.5.11.
    /// Domain exceptions are translated to HTTP responses by a global exception-handling
    /// middleware (not shown here) so controller actions stay free of try/catch noise —
    /// see Exceptions/DomainExceptions.cs for the exception -> status code mapping.
    /// </summary>
    [ApiController]
    public class ParkingController : ControllerBase
    {
        private readonly IParkingService _parkingService;

        public ParkingController(IParkingService parkingService)
        {
            _parkingService = parkingService;
        }

        /// <summary>GET /api/events/{eventId}/parking-slots — layout with live status. Any authenticated user.</summary>
        [HttpGet("api/events/{eventId:int}/parking-slots")]
        public async Task<ActionResult<List<ParkingSlotDto>>> GetLayout(int eventId)
        {
            var layout = await _parkingService.GetLayoutAsync(eventId);
            return Ok(layout);
        }

        /// <summary>POST /api/events/{eventId}/parking-slots — create layout + fee (Admin only, Rule #3).</summary>
        [HttpPost("api/events/{eventId:int}/parking-slots")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ParkingSlotDto>>> CreateLayout(
            int eventId, [FromBody] ParkingLayoutCreateDto dto)
        {
            var created = await _parkingService.CreateLayoutAsync(eventId, dto);
            return StatusCode(201, created);
        }

        /// <summary>PUT /api/events/{eventId}/parking-slots/{slotId} — edit slot (Admin only).</summary>
        [HttpPut("api/events/{eventId:int}/parking-slots/{slotId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkingSlotDto>> UpdateSlot(
            int eventId, int slotId, [FromBody] ParkingSlotUpdateDto dto)
        {
            var updated = await _parkingService.UpdateSlotAsync(eventId, slotId, dto);
            return Ok(updated);
        }

        /// <summary>DELETE /api/events/{eventId}/parking-slots/{slotId} — remove slot (Admin only, Rule #4).</summary>
        [HttpDelete("api/events/{eventId:int}/parking-slots/{slotId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveSlot(int eventId, int slotId)
        {
            await _parkingService.RemoveSlotAsync(eventId, slotId);
            return Ok();
        }

        /// <summary>POST /api/bookings/{bookingId}/parking — reserve a slot (Customer, race-safe, Rule #1).</summary>
        [HttpPost("api/bookings/{bookingId:int}/parking")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<ParkingReservationDto>> Reserve(
            int bookingId, [FromBody] ParkingReserveDto dto)
        {
            var reservation = await _parkingService.ReserveAsync(bookingId, dto);
            return StatusCode(201, reservation);
        }

        /// <summary>DELETE /api/bookings/{bookingId}/parking — remove reservation before finalization.</summary>
        [HttpDelete("api/bookings/{bookingId:int}/parking")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RemoveReservation(int bookingId)
        {
            await _parkingService.RemoveReservationAsync(bookingId);
            return Ok();
        }
    }
}
