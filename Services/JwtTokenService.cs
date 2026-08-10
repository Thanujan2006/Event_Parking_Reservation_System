using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Event_Parking_Reservation_System.Configuration;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Event_Parking_Reservation_System.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public string CreateToken(Customer customer)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                new("customerId", customer.CustomerId.ToString()),
                new(ClaimTypes.Email, customer.Email),
                new(ClaimTypes.Name, customer.Name)
            };

            // Controllers inconsistently use "Admin" and "Administrator".
            // Emit both so either authorize attribute succeeds.
            var role = string.IsNullOrWhiteSpace(customer.Role) ? "Customer" : customer.Role.Trim();
            claims.Add(new Claim(ClaimTypes.Role, role));
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
            else if (string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase))
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes <= 0 ? 120 : _settings.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
