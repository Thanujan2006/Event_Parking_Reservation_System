using Event_Parking_Reservation_System.Exceptions;
using System.Net;
using System.Text.Json;

namespace Event_Parking_Reservation_System.Middleware
{
    public class BookingExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BookingExceptionHandlingMiddleware> _logger;

        public BookingExceptionHandlingMiddleware(RequestDelegate next, ILogger<BookingExceptionHandlingMiddleware> logger)
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
            catch (BookingDomainException ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)MapStatusCode(ex);

                var body = JsonSerializer.Serialize(new { error = ex.ErrorCode, message = ex.Message });
                await context.Response.WriteAsync(body);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Booking module.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = "INTERNAL_ERROR", message = "An unexpected error occurred." }));
            }
        }

        private static HttpStatusCode MapStatusCode(BookingDomainException ex) => ex switch
        {
            EmptySeatListException => HttpStatusCode.BadRequest,
            BookingExpiredException => HttpStatusCode.BadRequest,
            BookingNotFoundException => HttpStatusCode.NotFound,
            BookingForbiddenException => HttpStatusCode.Forbidden,
            SeatAlreadyBookedException => HttpStatusCode.Conflict,
            ParkingSlotAlreadyReservedException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.BadRequest
        };
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseBookingExceptionHandling(this IApplicationBuilder app) =>
            app.UseMiddleware<BookingExceptionHandlingMiddleware>();
    }
}

