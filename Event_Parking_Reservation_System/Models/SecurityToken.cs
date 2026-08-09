using Event_Parking_Reservation_System.Interfaces;

namespace Event_Parking_Reservation_System.Models
{
    public class SecurityToken
    {
        public string TokenHash { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; }

        // EF Core owned-type constructor.
        private SecurityToken() { }


        public static (string rawToken, SecurityToken token) Generate(
            TimeSpan validFor,
            ITokenGenarater generator,
            ITokenHasher hasher)
        {
            var raw = generator.GenerateToken();
            var token = new SecurityToken
            {
                TokenHash = hasher.Hash(raw),
                ExpiresAt = DateTime.UtcNow.Add(validFor),
                IsUsed = false
            };
            return (raw, token);
        }

    
        public bool IsValid(string rawToken, ITokenHasher hasher)
        {
            if (IsUsed) return false;
            if (DateTime.UtcNow > ExpiresAt) return false;
            return hasher.Verify(rawToken, TokenHash);
        }

        public void Invalidate() => IsUsed = true;
    }


}

