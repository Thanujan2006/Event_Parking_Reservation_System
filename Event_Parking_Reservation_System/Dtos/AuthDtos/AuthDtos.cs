using System.ComponentModel.DataAnnotations;

namespace Event_Parking_Reservation_System.Dtos.AuthDtos
{
    public class AuthDtos
    {
        public class ForgotPasswordRequest
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        /// <summary>BRD Rule #8: always 200 with this exact generic message, regardless of outcome.</summary>
        public class ForgotPasswordResponse
        {
            public string Message { get; } = "If this email is registered, a password reset link has been sent.";
        }

        public class ResetPasswordRequest
        {
            [Required]
            public string Token { get; set; } = string.Empty;

            [Required]
            public string Email { get; set; } = string.Empty;

            // BRD 4.9.12: same complexity rules as registration (Module 1) —
            // enforced by [RegularExpression] here and re-validated by
            // Module 1's password policy when the hash is actually set.
            [Required]
            [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
            [RegularExpression(@"^(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
                ErrorMessage = "Password must contain at least one number and one special character.")]
            public string NewPassword { get; set; } = string.Empty;
        }

        public class ResendVerificationRequest
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        public class VerifyEmailResponse
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
