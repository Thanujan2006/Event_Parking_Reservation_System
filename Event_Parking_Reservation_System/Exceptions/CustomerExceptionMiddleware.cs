using System.Net;
using System.Text.Json;
using static Event_Parking_Reservation_System.Exceptions.CustomerExceptions;

namespace Event_Parking_Reservation_System.Exceptions
{
    public class CustomerExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomerExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AccountDeactivatedException ex)
            {
                await WriteJsonAsync(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (DuplicateEmailException ex)
            {
                await WriteJsonAsync(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (CustomerNotFoundException ex)
            {
                await WriteJsonAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (EmailChangeNotAllowedException ex)
            {
                await WriteJsonAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ActiveBookingsExistException ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    message = ex.Message,
                    activeBookingIds = ex.BookingIds
                }));
            }
        }

        private static Task WriteJsonAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
        }
    }
}
