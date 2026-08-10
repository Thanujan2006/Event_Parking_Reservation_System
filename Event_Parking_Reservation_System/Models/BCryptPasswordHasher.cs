using Event_Parking_Reservation_System.Interfaces;

namespace Event_Parking_Reservation_System.Models
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string plainTextPassword) =>
            BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

        public bool Verify(string plainTextPassword, string passwordHash) =>
            BCrypt.Net.BCrypt.Verify(plainTextPassword, passwordHash);
    }
}
