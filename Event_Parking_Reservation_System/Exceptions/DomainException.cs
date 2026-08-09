namespace Event_Parking_Reservation_System.Exceptions
{
    /// <summary>Base type so the global exception middleware can pattern-match once.</summary>
    public abstract class DomainException : Exception
    {
        public string ErrorCode { get; }
        protected DomainException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>Maps to 404 Not Found.</summary>
    public class NotFoundException : DomainException
    {
        public NotFoundException(string errorCode, string message) : base(errorCode, message) { }
    }

    /// <summary>Maps to 400 Bad Request.</summary>
    public class BadRequestException : DomainException
    {
        public BadRequestException(string errorCode, string message) : base(errorCode, message) { }
    }

    /// <summary>Maps to 409 Conflict — used for concurrent double-reservation, duplicate slot numbers, etc.</summary>
    public class ConflictException : DomainException
    {
        public ConflictException(string errorCode, string message) : base(errorCode, message) { }
    }

    /// <summary>Maps to 403 Forbidden.</summary>
    public class ForbiddenException : DomainException
    {
        public ForbiddenException(string errorCode, string message) : base(errorCode, message) { }
    }
}
