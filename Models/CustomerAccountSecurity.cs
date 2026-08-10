using Event_Parking_Reservation_System.Interfaces;
using Microsoft.IdentityModel.Tokens;
using static Event_Parking_Reservation_System.Exceptions.AuthExceptions;

namespace Event_Parking_Reservation_System.Models
{
    public class CustomerAccountSecurity
    {
        public int id { get; set; }
        public bool EmailVerified { get; private set; }
        public SecurityToken? EmailVerificationToken { get; private set; }
        public SecurityToken? PasswordResetToken { get; private set; }

        private CustomerAccountSecurity() { }

        /// <summary>Called once, at registration (BRD Rule #1: starts Unverified).</summary>
        public static CustomerAccountSecurity NewUnverified() => new CustomerAccountSecurity
        {
            EmailVerified = false
        };

        /// <summary>Simple beginner flow: account is usable immediately after register.</summary>
        public static CustomerAccountSecurity NewActive() => new CustomerAccountSecurity
        {
            EmailVerified = true
        };

        /// <summary>
        /// BRD Rule #2 / #3: issues a new verification token, invalidating
        /// any previous one. Used both at registration and on resend.
        /// </summary>
        public string IssueEmailVerificationToken(TimeSpan validFor, ITokenGenarater generator, ITokenHasher hasher)
        {
            if (EmailVerified)
                throw new AlreadyVerifiedException();

            var (raw, token) = SecurityToken.Generate(validFor, generator, hasher);
            EmailVerificationToken = token;
            return raw;
        }

        /// <summary>BRD 4.9.7: GET /api/auth/verify-email?token=</summary>
        public void VerifyEmail(string rawToken, ITokenHasher hasher)
        {
            if (EmailVerificationToken is null || !EmailVerificationToken.IsValid(rawToken, hasher))
                throw new InvalidOrExpiredTokenException();

            EmailVerificationToken.Invalidate();
            EmailVerified = true;
        }

        /// <summary>
        /// BRD Rule #4/#5: issues a password reset token. Unlike email
        /// verification, this is always callable — even mid-reset — so a
        /// customer who lost their first reset email can request another;
        /// the earlier token simply becomes unreachable (overwritten) and
        /// is no longer valid.
        /// </summary>
        public string IssuePasswordResetToken(TimeSpan validFor, ITokenGenarater generator, ITokenHasher hasher)
        {
            var (raw, token) = SecurityToken.Generate(validFor, generator, hasher);
            PasswordResetToken = token;
            return raw;
        }

        /// <summary>
        /// BRD Rule #6/#7: validates the token only — the caller (AuthService)
        /// is responsible for actually changing the password hash, since
        /// that touches the Customer's Module-1-owned PasswordHash field.
        /// </summary>
        public void ConsumePasswordResetToken(string rawToken, ITokenHasher hasher)
        {
            if (PasswordResetToken is null || !PasswordResetToken.IsValid(rawToken, hasher))
                throw new InvalidOrExpiredTokenException();

            PasswordResetToken.Invalidate();
        }
    }
}
