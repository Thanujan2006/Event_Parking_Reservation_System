
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
    [Route("api/venues")]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        /// <summary>
        /// Admin: create venue. 201 Created on success.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<VenueSummaryDto>> Create(
            CreateVenueRequest request)
        {
            var result = await _venueService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetDetail),
                new { venueId = result.VenueId },
                result);
        }

        /// <summary>
        /// List all venues.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll()
        {
            var venues = await _venueService.GetAllAsync();

            return Ok(venues);
        }

        /// <summary>
        /// Returns venues with no overlapping event in
        /// [startTime, endTime).
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<ActionResult> GetAvailable(
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            try
            {
                var venues = await _venueService.GetAvailableAsync(
                    startTime,
                    endTime);

                return Ok(venues);
            }
            catch (InvalidTimeRangeException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Returns full venue details.
        /// </summary>
        [HttpGet("{venueId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<VenueDetailDto>> GetDetail(
            int venueId)
        {
            try
            {
                var venue = await _venueService.GetDetailAsync(venueId);

                return Ok(venue);
            }
            catch (VenueNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Admin: update venue.
        /// </summary>
        [HttpPut("{venueId:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<VenueDetailDto>> Update(
            int venueId,
            UpdateVenueRequest request)
        {
            try
            {
                var venue = await _venueService.UpdateAsync(
                    venueId,
                    request);

                return Ok(venue);
            }
            catch (VenueNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Admin: delete venue.
        /// Returns 409 if the venue has upcoming events.
        /// </summary>
        [HttpDelete("{venueId:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Delete(int venueId)
        {
            try
            {
                await _venueService.DeleteAsync(venueId);

                return NoContent();
            }
            catch (VenueHasUpcomingEventsException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (VenueNotFoundException)
            {
                return NotFound();
            }
        }
    }


    [ApiController]
    [Route("api/categories")]
    public class EventCategoriesController : ControllerBase
    {
        private readonly IEventCategoryService _categoryService;

        public EventCategoriesController(
            IEventCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Admin: create category.
        /// Returns 409 if category name already exists.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<CategoryDto>> Create(
            CreateCategoryRequest request)
        {
            try
            {
                var result = await _categoryService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetAll),
                    new { },
                    result);
            }
            catch (DuplicateCategoryNameException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// List all categories.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Admin: update category.
        /// </summary>
        [HttpPut("{categoryId:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<CategoryDto>> Update(
            int categoryId,
            UpdateCategoryRequest request)
        {
            try
            {
                var category = await _categoryService.UpdateAsync(
                    categoryId,
                    request);

                return Ok(category);
            }
            catch (CategoryNotFoundException)
            {
                return NotFound();
            }
            catch (DuplicateCategoryNameException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Admin: delete category.
        /// Returns 409 if category is currently in use.
        /// </summary>
        [HttpDelete("{categoryId:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Delete(int categoryId)
        {
            try
            {
                await _categoryService.DeleteAsync(categoryId);

                return NoContent();
            }
            catch (CategoryInUseException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (CategoryNotFoundException)
            {
                return NotFound();
            }
        }
    }
}

