using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Event_Parking_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {

        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET /api/dashboard/customer/{customerId}
        [HttpGet("customer/{customerId:int}")]
        public async Task<IActionResult> GetCustomerDashboard(int customerId)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!isAdmin && currentId != customerId.ToString())
                return Forbid(); // or return Unauthorized(); depending on your semantics

            var result = await _dashboardService.GetCustomerDashboardAsync(customerId);
            return Ok(result);
        }

        // GET /api/dashboard/admin
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var result = await _dashboardService.GetAdminDashboardAsync();
            return Ok(result);
        }



    }
}
