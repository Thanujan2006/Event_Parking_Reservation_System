using Event_Parking_Reservation_System.Dtos.BookingDtos;
using Event_Parking_Reservation_System.Dtos.NotificationDtos;
using Event_Parking_Reservation_System.Dtos.PaymentDtos;
using Event_Parking_Reservation_System.DTOs;

namespace Event_Parking_Reservation_System.Dtos.DashBoardDtos
{
    public class DashboardDtos
    {


        public record CustomerDashboardResponse
        {
            public List<CreateBookingResponse> UpcomingBookings { get; init; }
            public List<ParkingReserveDto> ReservedParking { get; init; }
            public List<PaymentResponse> RecentPayments { get; init; }
            public int UnreadNotificationCount { get; init; }
            public List<CreateNotificationRequest> UnreadNotifications { get; init; }

            private List<BookingDtos.BookingDtos> upcoming;
            private object reservedParking;
            private List<PaymentHistoryDtos> recentPayments;
            private int count;
            private List<NotificationDto> unread;

            public CustomerDashboardResponse(List<BookingDtos.BookingDtos> upcoming, List<int?> reservedParking, List<PaymentHistoryDtos> recentPayments, int count, List<NotificationDto> unread)
            {
                this.upcoming = upcoming;
                this.reservedParking = reservedParking;
                this.recentPayments = recentPayments;
                this.count = count;
                this.unread = unread;
            }
        }

        public record AdminDashboardResponse(
            int TotalEvents,
            int TotalBookings,
            int TotalAvailableSeats,
            int TotalOccupiedParkingSlots,
            decimal TotalRevenue,
            int TotalCustomers
        );
        



    }
}
