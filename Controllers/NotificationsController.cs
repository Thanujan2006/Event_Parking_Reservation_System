using Event_Parking_Reservation_System.Dtos.NotificationDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Event_Parking_Reservation_System.Exceptions.NotificationExceptions;
using static Event_Parking_Reservation_System.Interfaces.INotificationService;

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

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<NotificationDto>> Create([FromBody] CreateNotificationRequest request)
        {
            var notification = await _publisher.CreateAsync(request.CustomerId, request.Type, request.Message);
            return CreatedAtAction(nameof(Create), new { id = notification.NotificationId }, NotificationDto.FromEntity(notification));
        }

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

        private int GetAuthenticatedCustomerId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var customerId) ? customerId : 0;
        }
    }
}
