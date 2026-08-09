
using Event_Parking_Reservation_System.DTOs;
using Event_Parking_Reservation_System.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISeatService = Event_Parking_Reservation_System.Interfaces.ISeatService;


namespace Event_Parking_Reservation_System.Controllers
{
    [ApiController]
    [Route("api/events/{eventId:int}/seats")]
    public class EventSeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public EventSeatsController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        /// <summary>Admin: generate seat map. AC2 — 400 if Rows*Columns != event capacity.</summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> GenerateSeatMap(int eventId, GenerateSeatMapRequest request)
        {
            try
            {
                await _seatService.GenerateSeatMapAsync(eventId, request);
                return CreatedAtAction(nameof(GetSeatMap), new { eventId }, null);
            }
            catch (SeatCountMismatchException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (SeatMapAlreadyExistsException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (EventNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>Visual seat map grid (BRD 4.4.2 — status color-coded on the frontend).</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<SeatMapDto>> GetSeatMap(int eventId)
        {
            try
            {
                return Ok(await _seatService.GetSeatMapAsync(eventId));
            }
            catch (EventNotFoundException)
            {
                return NotFound();
            }
        }
    }

    [ApiController]
    [Route("api/bookings/{bookingId:int}/seats")]
    public class BookingSeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public BookingSeatsController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        /// <summary>AC1 — 200 OK with held seats, or 409 Conflict 'Seat already booked' if any seat lost the race.</summary>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<HoldSeatsResult>> HoldSeats(int bookingId, HoldSeatsRequest request)
        {
            try
            {
                var result = await _seatService.HoldSeatsAsync(bookingId, request);
                return Ok(result);
            }
            catch (SeatsUnavailableException ex)
            {
                return Conflict(new
                {
                    message = "Seat already booked",
                    unavailableSeatIds = ex.UnavailableSeatIds
                });
            }
        }
    }

    [ApiController]
    [Route("api/seats")]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public SeatsController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        /// <summary>AC3 — 409 Conflict if the seat has an active (Held/Booked) booking.</summary>
        [HttpDelete("{seatId:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Delete(int seatId)
        {
            try
            {
                await _seatService.DeleteSeatAsync(seatId);
                return NoContent();
            }
            catch (SeatHasActiveBookingException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (SeatNotFoundException)
            {
                return NotFound();
            }
        }
    }

}

