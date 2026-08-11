using FluentValidation;
using static Event_Parking_Reservation_System.Dtos.CustomerDtos.CustomerDtos;

namespace Event_Parking_Reservation_System.Vaildators
{
    public class CustomerVaildators
    {
        public class RegisterCustomerRequestValidator : AbstractValidator<RegisterCustomerRequest>
        {
            public RegisterCustomerRequestValidator()
            {
                RuleFor(x => x)
                    .Must(x => !string.IsNullOrWhiteSpace(x.ResolvedName))
                    .WithMessage("Name is required.")
                    .Must(x => (x.ResolvedName?.Trim().Length ?? 0) >= 2)
                    .WithMessage("Name must be at least 2 characters.");

                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("Email must be a valid, RFC-compliant address.");

                RuleFor(x => x)
                    .Must(x => !string.IsNullOrWhiteSpace(x.ResolvedPhone))
                    .WithMessage("Phone number is required.")
                    .Must(x => System.Text.RegularExpressions.Regex.IsMatch(x.ResolvedPhone ?? "", @"^\d{10}$"))
                    .WithMessage("Phone number must be exactly 10 digits.");

                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password is required.")
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                    .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                    .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

                RuleFor(x => x.ConfirmPassword)
                    .Must((req, confirm) =>
                        string.IsNullOrWhiteSpace(confirm) || confirm == req.Password)
                    .WithMessage("Password and confirmation do not match.");
            }
        }

        /// <summary>
        /// Profile update validation. Email is deliberately not part of this DTO
        /// (see UpdateCustomerProfileRequest) so it can never be changed here.
        /// </summary>
        public class UpdateCustomerProfileRequestValidator : AbstractValidator<UpdateCustomerProfileRequest>
        {
            public UpdateCustomerProfileRequestValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Name is required.")
                    .MinimumLength(2).WithMessage("Name must be at least 2 characters.");

                RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required.")
                    .Matches(@"^\d{10}$").WithMessage("Phone number must be exactly 10 digits.");
            }
        }
    }
}
    
