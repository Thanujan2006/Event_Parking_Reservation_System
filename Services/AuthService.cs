using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using Microsoft.Extensions.Options;
using static Event_Parking_Reservation_System.Dtos.AuthDtos.AuthDtos;
using static Event_Parking_Reservation_System.Exceptions.AuthExceptions;
using static Event_Parking_Reservation_System.Exceptions.CustomerExceptions;

namespace Event_Parking_Reservation_System.Services
{
    public class AuthService : IAuthService
    {
        private readonly ICustomerAccountRepository _repository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAuthEmailSender _emailSender;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenarater _tokenGenerator;
        private readonly ITokenHasher _tokenHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly AuthTokenOptions _options;

        public AuthService(
            ICustomerAccountRepository repository,
            ICustomerRepository customerRepository,
            IAuthEmailSender emailSender,
            IPasswordHasher passwordHasher,
            ITokenGenarater tokenGenerator,
            ITokenHasher tokenHasher,
            IJwtTokenService jwtTokenService,
            IOptions<AuthTokenOptions> options)
        {
            _repository = repository;
            _customerRepository = customerRepository;
            _emailSender = emailSender;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _tokenHasher = tokenHasher;
            _jwtTokenService = jwtTokenService;
            _options = options.Value;
        }

        /// <summary>BRD 4.9.7 / AC1: called right after registration by Module 1.</summary>
        public async Task<string> IssueEmailVerificationTokenForNewCustomerAsync(int customerId, string email)
        {
            var record = await _repository.GetByIdAsync(customerId)
                ?? throw new Exceptions.AuthExceptions.CustomerNotFoundException();

            var rawToken = record.Security.IssueEmailVerificationToken(
                _options.EmailVerificationLifetime, _tokenGenerator, _tokenHasher);

            await _repository.SaveSecurityStateAsync(customerId, record.Security);
            await _emailSender.SendVerificationEmailAsync(email, rawToken);
            return rawToken;
        }

        /// <summary>BRD 4.9.7 / AC3-AC4: GET /api/auth/verify-email?token=</summary>
        public async Task VerifyEmailAsync(string token)
        {
            var tokenHash = _tokenHasher.Hash(token);
            var record = await _repository.GetByEmailVerificationTokenHashAsync(tokenHash)
                ?? throw new InvalidOrExpiredTokenException();

            record.Security.VerifyEmail(token, _tokenHasher);
            await _repository.MarkEmailVerifiedAsync(record.CustomerId, record.Security);
        }

        /// <summary>
        /// BRD Rule #3 / 4.9.13: invalidates the previous token and issues
        /// a fresh one. Rejects if already verified (matches the exception
        /// table's "Verified Customer-க்கு Resend Verification Attempt").
        /// </summary>
        public async Task ResendVerificationAsync(string email)
        {
            var record = await _repository.GetByEmailAsync(email)
                ?? throw new Exceptions.AuthExceptions.CustomerNotFoundException();

            var rawToken = record.Security.IssueEmailVerificationToken(
                _options.EmailVerificationLifetime, _tokenGenerator, _tokenHasher);

            await _repository.SaveSecurityStateAsync(record.CustomerId, record.Security);
            await _emailSender.SendVerificationEmailAsync(email, rawToken);
        }

        /// <summary>
        /// BRD Rule #8 / AC5: intentionally never surfaces whether the
        /// email exists. If it doesn't, this is a silent no-op — the
        /// controller returns the same 200 generic message either way.
        /// </summary>
        public async Task ForgotPasswordAsync(string email)
        {
            var record = await _repository.GetByEmailAsync(email);
            if (record is null)
                return; // Deliberately silent — see BRD Rule #8.

            var rawToken = record.Security.IssuePasswordResetToken(
                _options.PasswordResetLifetime, _tokenGenerator, _tokenHasher);

            await _repository.SaveSecurityStateAsync(record.CustomerId, record.Security);
            await _emailSender.SendPasswordResetEmailAsync(email, rawToken);
        }

        /// <summary>BRD Rule #6/#7 / AC3-AC4: POST /api/auth/reset-password</summary>
        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            var record = await _repository.GetByEmailAsync(email)
                ?? throw new InvalidOrExpiredTokenException(); // Same generic error — no account enumeration here either.

            record.Security.ConsumePasswordResetToken(token, _tokenHasher);

            await _repository.SaveSecurityStateAsync(record.CustomerId, record.Security);
            await _repository.SetPasswordHashAsync(record.CustomerId, _passwordHasher.Hash(newPassword));
        }

        /// <summary>Token-only reset used by the frontend reset-password page.</summary>
        public async Task ResetPasswordByTokenAsync(string token, string newPassword)
        {
            var tokenHash = _tokenHasher.Hash(token);
            var record = await _repository.GetByPasswordResetTokenHashAsync(tokenHash)
                ?? throw new InvalidOrExpiredTokenException();

            record.Security.ConsumePasswordResetToken(token, _tokenHasher);
            await _repository.SaveSecurityStateAsync(record.CustomerId, record.Security);
            await _repository.SetPasswordHashAsync(record.CustomerId, _passwordHasher.Hash(newPassword));
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var customer = await _customerRepository.GetByEmailAsync(normalizedEmail);

            if (customer is null || !_passwordHasher.Verify(password, customer.PasswordHash))
                throw new InvalidCredentialsException();

            if (customer.Status == CustomerStatus.Deactivated)
                throw new AccountDeactivatedException();

            var role = string.IsNullOrWhiteSpace(customer.Role) ? "Customer" : customer.Role.Trim();
            // Frontend checks role === "Admin"
            if (string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase))
                role = "Admin";

            return new LoginResponse
            {
                Token = _jwtTokenService.CreateToken(customer),
                Customer = new LoginCustomerDto
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.Name,
                    Email = customer.Email,
                    Phone = customer.PhoneNumber,
                    Role = role,
                    Status = customer.Status.ToString()
                }
            };
        }
    }
}
