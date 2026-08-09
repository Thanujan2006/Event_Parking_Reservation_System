using System.Net;
using System.Text.Json;
using Event_Parking_Reservation_System.Exceptions;

namespace Event_Parking_Reservation_System.Middleware
{
    public class PaymentExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PaymentExceptionHandlingMiddleware> _logger;

        public PaymentExceptionHandlingMiddleware(RequestDelegate next, ILogger<PaymentExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (PaymentDomainException ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)MapStatusCode(ex);
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = ex.ErrorCode, message = ex.Message }));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Payment module.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = "INTERNAL_ERROR", message = "An unexpected error occurred." }));
            }
        }

        private static HttpStatusCode MapStatusCode(PaymentDomainException ex) => ex switch
        {
            PaymentBookingNotFoundException => HttpStatusCode.NotFound,
            PaymentNotFoundException => HttpStatusCode.NotFound,
            DuplicatePaymentException => HttpStatusCode.Conflict,
            BookingExpiredForPaymentException => HttpStatusCode.BadRequest,
            BookingNotPayableException => HttpStatusCode.BadRequest,
            PaymentForbiddenException => HttpStatusCode.Forbidden,
            _ => HttpStatusCode.BadRequest
        };
    }

    public static class PaymentExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UsePaymentExceptionHandling(this IApplicationBuilder app) =>
            app.UseMiddleware<PaymentExceptionHandlingMiddleware>();
    }
}

