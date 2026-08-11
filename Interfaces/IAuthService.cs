using Event_Parking_Reservation_System.Models;

namespace Event_Parking_Reservation_System.Interfaces
{
    /// <summary>
    /// Narrow bridge from Authentication to the Customer aggregate.
    /// The implementation belongs in the Customer Management module.
    /// </summary>
    public interface ICustomerAccountRepository
    {
        Task<CustomerAccountRecord?> GetByEmailAsync(string email);
        Task<CustomerAccountRecord?> GetByIdAsync(int customerId);
        Task<CustomerAccountRecord?> GetByEmailVerificationTokenHashAsync(string tokenHash);
        Task<CustomerAccountRecord?> GetByPasswordResetTokenHashAsync(string tokenHash);

        Task SetPasswordHashAsync(int customerId, string newPasswordHash);
        Task SaveSecurityStateAsync(int customerId, CustomerAccountSecurity security);
        Task MarkEmailVerifiedAsync(int customerId, CustomerAccountSecurity security);
    }

    public record CustomerAccountRecord(int CustomerId, string Email, CustomerAccountSecurity Security);

    public interface IPasswordHasher
    {
        string Hash(string plainTextPassword);
        bool Verify(string plainTextPassword, string passwordHash);
    }

    public interface IAuthEmailSender
    {
        Task SendVerificationEmailAsync(string toEmail, string verificationToken);
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    }

    public interface IAuthService
    {
        Task<string> IssueEmailVerificationTokenForNewCustomerAsync(int customerId, string email);
        Task VerifyEmailAsync(string token);
        Task ResendVerificationAsync(string email);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string email, string token, string newPassword);
        Task ResetPasswordByTokenAsync(string token, string newPassword);
        Task<Dtos.AuthDtos.AuthDtos.LoginResponse> LoginAsync(string email, string password);
    }
}
