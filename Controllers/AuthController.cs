using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using static Event_Parking_Reservation_System.Dtos.AuthDtos.AuthDtos;
using static Event_Parking_Reservation_System.Exceptions.AuthExceptions;
using static Event_Parking_Reservation_System.Exceptions.CustomerExceptions;

namespace Event_Parking_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] Dtos.AuthDtos.AuthDtos.ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request.Email.Trim());
            return Ok(new ForgotPasswordResponse());
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] Dtos.AuthDtos.AuthDtos.ResetPasswordRequest request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request.Email.Trim(), request.Token.Trim(), request.NewPassword);
                return Ok(new { message = "Password has been reset successfully." });
            }
            catch (InvalidOrExpiredTokenException ex)
            {
                return BadRequest(new { error = "INVALID_OR_EXPIRED_TOKEN", message = ex.Message });
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { error = "TOKEN_REQUIRED", message = "Verification token is required." });

            try
            {
                await _authService.VerifyEmailAsync(token.Trim());
                return Ok(new VerifyEmailResponse
                {
                    Message = "Email verified successfully. You can now log in."
                });
            }
            catch (InvalidOrExpiredTokenException ex)
            {
                return BadRequest(new { error = "INVALID_OR_EXPIRED_TOKEN", message = ex.Message });
            }
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            try
            {
                await _authService.ResendVerificationAsync(request.Email.Trim());
                return Ok(new { message = "Verification email resent." });
            }
            catch (Exceptions.AuthExceptions.CustomerNotFoundException)
            {
                return NotFound(new { message = "Customer not found." });
            }
            catch (AlreadyVerifiedException ex)
            {
                return BadRequest(new { error = "ALREADY_VERIFIED", message = ex.Message });
            }
        }
    }
}
