using Event_Parking_Reservation_System.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Event_Parking_Reservation_System.Models
{
    public class RequireVerifiedEmailFilter : IAsyncActionFilter
    {
        private readonly ICustomerAccountRepository _repository;

        public RequireVerifiedEmailFilter(ICustomerAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var customerId = ResolveAuthenticatedCustomerId(context.HttpContext);

            var record = customerId.HasValue
                ? await _repository.GetByIdAsync(customerId.Value)
                : null;

            if (record is null || !record.Security.EmailVerified)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(
                    new { error = "EMAIL_NOT_VERIFIED", message = "Please verify your email before continuing." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }

        private static int? ResolveAuthenticatedCustomerId(HttpContext httpContext)
        {
            var claim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
