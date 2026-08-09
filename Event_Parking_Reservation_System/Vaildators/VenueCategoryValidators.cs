using Event_Parking_Reservation_System.Dtos;
using FluentValidation;

namespace Event_Parking_Reservation_System.Validators
{
    /// <summary>
    /// BRD 4.2.12:
    /// - Venue Name: required, max 150 chars
    /// - Total Capacity: positive integer, 0 not allowed
    /// - Category Name: required, unique (uniqueness enforced in service layer)
    /// - Availability query: startTime < endTime
    /// </summary>
    public class CreateVenueRequestValidator : AbstractValidator<CreateVenueRequest>
    {
        public CreateVenueRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Venue name is required.")
                .MaximumLength(150).WithMessage("Venue name must not exceed 150 characters.");
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.");
            RuleFor(x => x.TotalCapacity)
                .GreaterThan(0).WithMessage("Total capacity must be a positive integer greater than 0.");
        }
    }

    public class UpdateVenueRequestValidator : AbstractValidator<UpdateVenueRequest>
    {
        public UpdateVenueRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Venue name is required.")
                .MaximumLength(150).WithMessage("Venue name must not exceed 150 characters.");
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.");
            RuleFor(x => x.TotalCapacity)
                .GreaterThan(0).WithMessage("Total capacity must be a positive integer greater than 0.");
        }
    }

    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
        }
    }

    public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
        }
    }

    public class VenueAvailabilityQueryValidator : AbstractValidator<VenueAvailabilityQuery>
    {
        public VenueAvailabilityQueryValidator()
        {
            RuleFor(x => x)
                .Must(x => x.StartTime < x.EndTime)
                .WithMessage("startTime must be earlier than endTime.");
        }
    }
}