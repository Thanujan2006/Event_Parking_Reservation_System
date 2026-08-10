using Event_Parking_Reservation_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>POST /api/auth/login — returns JWT + customer profile for the SPA.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request.Email, request.Password);
                return Ok(result);
            }
            catch (InvalidCredentialsException ex)
            {
                return Unauthorized(new { error = "INVALID_CREDENTIALS", message = ex.Message });
            }
            catch (AccountDeactivatedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { error = "ACCOUNT_DEACTIVATED", message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request.Email.Trim());
            return Ok(new ForgotPasswordResponse());
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    await _authService.ResetPasswordByTokenAsync(request.Token.Trim(), request.NewPassword);
                else
                    await _authService.ResetPasswordAsync(request.Email.Trim(), request.Token.Trim(), request.NewPassword);

                return Ok(new { message = "Password has been reset successfully." });
            }
            catch (InvalidOrExpiredTokenException ex)
            {
                return BadRequest(new { error = "INVALID_OR_EXPIRED_TOKEN", message = ex.Message });
            }
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
