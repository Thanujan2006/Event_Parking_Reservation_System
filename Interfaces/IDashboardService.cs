using static Event_Parking_Reservation_System.Dtos.DashBoardDtos.DashboardDtos;


   

namespace Event_Parking_Reservation_System.Interfaces
    {
        public interface IDashboardService
        {
            Task<CustomerDashboardResponse> GetCustomerDashboardAsync(int customerId);

            Task<AdminDashboardResponse> GetAdminDashboardAsync();
        }
    }



