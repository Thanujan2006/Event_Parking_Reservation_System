namespace Event_Parking_Reservation_System.Interfaces
{
    public interface ITokenGenarater
    {

        string GenerateToken();
    }

    /// <summary>
    /// Hashes tokens for storage and verifies a raw token against a stored
    /// hash. Kept separate from password hashing (Module 1) even though
    /// the mechanics are similar — tokens and passwords have different
    /// threat models (tokens are single-use and short-lived).
    /// </summary>
    public interface ITokenHasher
    {
        string Hash(string rawToken);
        bool Verify(string rawToken, string tokenHash);
    }

}

