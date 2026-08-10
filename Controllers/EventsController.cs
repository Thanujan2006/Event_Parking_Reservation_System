using System;
using System.Threading.Tasks;
using Event_Parking_Reservation_System.Dtos;
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_Parking_Reservation_System.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Admin: create event.
        /// AC1 — 409 Conflict on venue schedule overlap.
        /// AC2 — 400 Bad Request if capacity exceeds venue capacity.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Administrator")]
        public async Task<ActionResult<EventDetailDto>> Create(CreateEventRequest request)
        {
            try
            {
                var result = await _eventService.CreateAsync(request);
                return CreatedAtAction(nameof(GetDetail), new { eventId = result.EventId }, result);
            }
            catch (VenueScheduleOverlapException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (EventCapacityExceedsVenueException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (VenueNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (CategoryNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Customer-facing browse/search (BRD 4.3.2 — Name, Date, Venue, Category).</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Search([FromQuery] EventSearchQuery query)
        {
            var result = await _eventService.SearchAsync(query);
            return Ok(result);
        }

        [HttpGet("{eventId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<EventDetailDto>> GetDetail(int eventId)
        {
            try
            {
                return Ok(await _eventService.GetDetailAsync(eventId));
            }
            catch (EventNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{eventId:int}")]
        [Authorize(Roles = "Admin,Administrator")]
        public async Task<ActionResult<EventDetailDto>> Update(int eventId, UpdateEventRequest request)
        {
            try
            {
                return Ok(await _eventService.UpdateAsync(eventId, request));
            }
            catch (VenueScheduleOverlapException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (EventCapacityExceedsVenueException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (VenueNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (CategoryNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (EventNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{eventId:int}")]
        [Authorize(Roles = "Admin,Administrator")]
        public async Task<ActionResult> Delete(int eventId)
        {
            try
            {
                await _eventService.DeleteAsync(eventId);
                return NoContent();
            }
            catch (EventNotFoundException)
            {
                return NotFound();
            }
        }
    }
}