using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_Parking_Reservation_System.Models
{
    public class AuthEntityConfiguration
    {
        public static void ConfigureSecurity<TOwner>(OwnedNavigationBuilder<TOwner, CustomerAccountSecurity> security)
          where TOwner : class
        {
            security.Property(s => s.EmailVerified)
                .HasColumnName("EmailVerified")
                .HasDefaultValue(false);

            security.OwnsOne(s => s.EmailVerificationToken, token =>
            {
                token.Property(t => t.TokenHash).HasColumnName("EmailVerificationToken").HasMaxLength(512);
                token.Property(t => t.ExpiresAt).HasColumnName("EmailVerificationTokenExpiresAt");
                token.Property(t => t.IsUsed).HasColumnName("EmailVerificationTokenUsed").HasDefaultValue(false);
            });

            security.OwnsOne(s => s.PasswordResetToken, token =>
            {
                token.Property(t => t.TokenHash).HasColumnName("PasswordResetToken").HasMaxLength(512);
                token.Property(t => t.ExpiresAt).HasColumnName("PasswordResetTokenExpiresAt");
                token.Property(t => t.IsUsed).HasColumnName("PasswordResetTokenUsed").HasDefaultValue(false);
            });
        }
    }
}
