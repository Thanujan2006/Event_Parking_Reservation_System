using Event_Parking_Reservation_System.Interfaces;
using System.Security.Cryptography;

namespace Event_Parking_Reservation_System.Models
{
    public class TokenSecurity
    {

        public class CryptoTokenGenerator : ITokenGenarater
        {
            private const int TokenBytes = 32;

            public string GenerateToken()
            {
                var bytes = RandomNumberGenerator.GetBytes(TokenBytes);
                return Convert.ToBase64String(bytes)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .TrimEnd('=');
            }
        }

        /// <summary>
        /// SHA-256 hash of the raw token. Tokens are already high-entropy
        /// random values (unlike passwords), so a fast cryptographic hash is
        /// appropriate here — no need for a slow KDF like BCrypt/PBKDF2,
        /// which BRD 7.1 reserves for passwords specifically.
        /// </summary>
        public class Sha256TokenHasher : ITokenHasher
        {
            public string Hash(string rawToken)
            {
                var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
                return Convert.ToHexString(bytes);
            }

            public bool Verify(string rawToken, string tokenHash)
            {
                var computed = Hash(rawToken);
                return CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(computed),
                    System.Text.Encoding.UTF8.GetBytes(tokenHash));
            }
        }
        

    }
}

