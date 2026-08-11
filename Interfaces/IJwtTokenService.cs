using Event_Parking_Reservation_System.Models;

namespace Event_Parking_Reservation_System.Interfaces
{
    public interface IJwtTokenService
    {
        string CreateToken(Customer customer);
    }
}
