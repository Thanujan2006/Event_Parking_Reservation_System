using Event_Parking_Reservation_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Event_Parking_Reservation_System.Dtos.CustomerDtos.CustomerDtos;
using static Event_Parking_Reservation_System.Exceptions.CustomerExceptions;

namespace Event_Parking_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterCustomerResponse>> Register([FromBody] RegisterCustomerRequest request)
        {
            try
            {
                var result = await _customerService.RegisterAsync(request);
                return Created($"/api/customers/{result.CustomerId}", result);
            }
            catch (DuplicateEmailException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>BRD endpoint: GET /api/customers/{id}.</summary>
        [HttpGet("{customerId:int}")]
        [Authorize]
        public async Task<ActionResult> GetProfile(int customerId)
        {
            if (!User.IsInRole("Administrator") && GetAuthenticatedCustomerId() != customerId)
                return Forbid();

            try
            {
                if (User.IsInRole("Administrator"))
                    return Ok(await _customerService.GetDetailAsync(customerId));

                return Ok(await _customerService.GetOwnProfileAsync(customerId));
            }
            catch (CustomerNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>Convenience route for the logged-in customer.</summary>
        [HttpGet("me")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<CustomerProfileDto>> GetOwnProfile()
        {
            var customerId = GetAuthenticatedCustomerId();
            if (customerId <= 0) return Unauthorized();
            return Ok(await _customerService.GetOwnProfileAsync(customerId));
        }

        /// <summary>BRD endpoint: PUT /api/customers/{id}.</summary>
        [HttpPut("{customerId:int}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<CustomerProfileDto>> UpdateProfile(
            int customerId, [FromBody] UpdateCustomerProfileRequest request)
        {
            if (GetAuthenticatedCustomerId() != customerId)
                return Forbid();

            try
            {
                return Ok(await _customerService.UpdateOwnProfileAsync(customerId, request));
            }
            catch (CustomerNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("me")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<CustomerProfileDto>> UpdateOwnProfile([FromBody] UpdateCustomerProfileRequest request)
        {
            var customerId = GetAuthenticatedCustomerId();
            if (customerId <= 0) return Unauthorized();
            return Ok(await _customerService.UpdateOwnProfileAsync(customerId, request));
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Search(
            [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _customerService.SearchAsync(search, page, pageSize);
            return Ok(new { items, total, page = System.Math.Max(1, page), pageSize = System.Math.Clamp(pageSize, 1, 100) });
        }

        /// <summary>BRD endpoint: DELETE /api/customers/{id} deactivates the account.</summary>
        [HttpDelete("{customerId:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Deactivate(int customerId)
        {
            try
            {
                await _customerService.DeactivateAsync(customerId);
                return NoContent();
            }
            catch (ActiveBookingsExistException ex)
            {
                return BadRequest(new { message = ex.Message, activeBookingIds = ex.BookingIds });
            }
            catch (CustomerNotFoundException)
            {
                return NotFound();
            }
        }

        // Kept as a compatibility alias for code/frontends that used the old route.
        [HttpPost("{customerId:int}/deactivate")]
        [Authorize(Roles = "Administrator")]
        public Task<ActionResult> DeactivateAlias(int customerId) => Deactivate(customerId);

        [HttpPost("{customerId:int}/reactivate")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Reactivate(int customerId)
        {
            try
            {
                await _customerService.ReactivateAsync(customerId);
                return NoContent();
            }
            catch (CustomerNotFoundException)
            {
                return NotFound();
            }
        }

        private int GetAuthenticatedCustomerId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("customerId")?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }
    }
}
