using System;
using Event_Parking_Reservation_System.Dtos;
using FluentValidation;

namespace Event_Parking_Reservation_System.Validators
{
    /// <summary>
    /// Field-level validation (BRD 4.3). Cross-entity rules — venue overlap and
    /// capacity-vs-venue-capacity — are enforced in the service layer since they
    /// require a database lookup (see EventService).
    /// </summary>
    public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
    {
        public CreateEventRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Event name is required.")
                .MaximumLength(200).WithMessage("Event name must not exceed 200 characters.");
            RuleFor(x => x.VenueId).GreaterThan(0).WithMessage("A valid venue must be selected.");
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("A valid category must be selected.");
            RuleFor(x => x.EventDateTime)
                .GreaterThan(DateTime.UtcNow).WithMessage("Event date/time must be in the future.");
            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Duration must be a positive number of minutes.");
            RuleFor(x => x.TicketPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Ticket price cannot be negative.");
            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be a positive integer.");
        }
    }

    public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
    {
        public UpdateEventRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Event name is required.")
                .MaximumLength(200).WithMessage("Event name must not exceed 200 characters.");
            RuleFor(x => x.VenueId).GreaterThan(0).WithMessage("A valid venue must be selected.");
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("A valid category must be selected.");
            RuleFor(x => x.EventDateTime)
                .GreaterThan(DateTime.UtcNow).WithMessage("Event date/time must be in the future.");
            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Duration must be a positive number of minutes.");
            RuleFor(x => x.TicketPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Ticket price cannot be negative.");
            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be a positive integer.");
        }
    }
}