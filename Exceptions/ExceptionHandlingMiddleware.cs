using System.Net;
using Event_Parking_Reservation_System.DTOs;
using Event_Parking_Reservation_System.Exceptions;


namespace Event_Parking_Reservation_System.Middleware
{
    /// <summary>
    /// Single place that maps DomainException subtypes to the HTTP status codes and
    /// response bodies specified in BRD 4.5.13 (Exception Handling) and Section 8
    /// (system-wide HTTP status code standard). Register with app.UseMiddleware&lt;...&gt;()
    /// early in the pipeline, before routing.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                context.Response.StatusCode = (int)MapStatusCode(ex);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ApiErrorResponse
                {
                    Error = ex.ErrorCode,
                    Message = ex.Message
                });
            }
        }

        private static HttpStatusCode MapStatusCode(DomainException ex) => ex switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            BadRequestException => HttpStatusCode.BadRequest,
            ConflictException => HttpStatusCode.Conflict,
            ForbiddenException => HttpStatusCode.Forbidden,
            _ => HttpStatusCode.InternalServerError
        };
    }
}
