using Event_Parking_Reservation_System.DTOs;
using FluentValidation;

namespace EventParkingSystem.Modules.SeatReservation.Validators
{
    public class GenerateSeatMapRequestValidator : AbstractValidator<GenerateSeatMapRequest>
    {
        public GenerateSeatMapRequestValidator()
        {
            RuleFor(x => x.Rows).GreaterThan(0).WithMessage("Rows must be a positive integer.");
            RuleFor(x => x.Columns).GreaterThan(0).WithMessage("Columns must be a positive integer.");

            RuleFor(x => x.PriceOverride)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PriceOverride.HasValue)
                .WithMessage("Price override cannot be negative.");
        }
    }

    /// <summary>BRD 4.4.12 — at least one seat is required per selection request.</summary>
    public class HoldSeatsRequestValidator : AbstractValidator<HoldSeatsRequest>
    {
        public HoldSeatsRequestValidator()
        {
            RuleFor(x => x.SeatIds)
                .NotEmpty().WithMessage("At least one seat must be selected.");

            RuleForEach(x => x.SeatIds)
                .GreaterThan(0).WithMessage("Seat IDs must be valid positive integers.");
        }
    }
}
