using Event_Parking_Reservation_System.Dtos.NotificationDtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using static Event_Parking_Reservation_System.Exceptions.NotificationExceptions;
using static Event_Parking_Reservation_System_Services.NotificationService;

namespace Event_Parking_Reservation_System.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationPublisher _publisher;
        private readonly INotificationQueryService _queryService;

        public NotificationsController(
            INotificationPublisher publisher,
            INotificationQueryService queryService)
        {
            _publisher = publisher;
            _queryService = queryService;
        }

        /// <summary>
        /// BRD 4.8.11 / AC3: internal service-to-service creation only.
        /// "ServiceOnly" is a policy that only an internal service-account
        /// JWT satisfies — a customer or admin token can never pass it,
        /// so any external caller gets 403 here, matching the BRD's
        /// exception table exactly.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "ServiceOnly")]
        public async Task<ActionResult<NotificationDto>> Create([FromBody] CreateNotificationRequest request)
        {
            var notification = await _publisher.CreateAsync(request.CustomerId, request.Type, request.Message);
            return CreatedAtAction(nameof(Create), new { id = notification.NotificationId }, NotificationDto.FromEntity(notification));
        }

        /// <summary>BRD 4.8.11: GET /api/notifications/customer/{customerId}.</summary>
        [HttpGet("customer/{customerId:int}")]
        public async Task<IActionResult> GetForCustomer(int customerId)
        {
            try
            {
                var notifications = await _queryService.GetForCustomerAsync(customerId, GetAuthenticatedCustomerId());
                return Ok(notifications);
            }
            catch (NotificationAccessDeniedException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>BRD 4.8.11: PUT /api/notifications/{id}/read.</summary>
        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                await _queryService.MarkAsReadAsync(id, GetAuthenticatedCustomerId());
                return Ok();
            }
            catch (NotificationNotFoundException)
            {
                return NotFound(new { message = "Notification not found." });
            }
            catch (NotificationAccessDeniedException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Reads the caller's own customerId out of the JWT — this is what
        /// makes GetForCustomer/MarkAsRead safe against a customer passing
        /// someone else's id in the URL (BRD Rule #1).
        /// </summary>
        private int GetAuthenticatedCustomerId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var customerId) ? customerId : 0;
        }
    }
}
