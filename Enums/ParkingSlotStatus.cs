namespace Event_Parking_Reservation_System.Enums
{
    /// <summary>
    /// Valid status transitions per BRD 4.5.12: Available -> Held -> Reserved.
    /// No other transition path is valid.
    /// </summary>
    public enum ParkingSlotStatus
    {
        Available = 0,
        Held = 1,
        Reserved = 2
    }
}
