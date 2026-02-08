using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;

namespace Second.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> RegisterAsync([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.RegisterAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(CreateProblemDetails("Registration failed.", ex.Message));
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.LoginAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(CreateProblemDetails("Login failed.", ex.Message));
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
        {
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

            if (string.IsNullOrWhiteSpace(jti) || string.IsNullOrWhiteSpace(expClaim) || !long.TryParse(expClaim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var exp))
            {
                return Unauthorized(CreateProblemDetails("Logout failed.", "Token metadata is missing."));
            }

            var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            await _authService.LogoutAsync(jti, expiresAtUtc, cancellationToken);
            ClearAuthCookies();

            return Ok(new { Message = "Logged out successfully." });
        }

        [HttpPost("request-email-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestEmailVerificationAsync([FromBody] RequestEmailVerificationRequest request, CancellationToken cancellationToken)
        {
            await _authService.RequestEmailVerificationAsync(request, cancellationToken);
            return Ok(new { Message = "If the account exists and is unverified, a verification email has been sent." });
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
        {
            var verified = await _authService.VerifyEmailAsync(request, cancellationToken);
            if (!verified)
            {
                return BadRequest(CreateProblemDetails("Verification failed.", "The token is invalid or expired."));
            }

            return Ok(new { Message = "Email verified successfully." });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            await _authService.RequestPasswordResetAsync(request, cancellationToken);
            return Ok(new { Message = "If an account exists, a reset link has been sent." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _authService.ResetPasswordAsync(request, cancellationToken);
                ClearAuthCookies();
                return Ok(new { Message = "Password reset successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(CreateProblemDetails("Reset password failed.", ex.Message));
            }
        }

        [HttpPost("become-seller")]
        [Authorize]
        public async Task<ActionResult<AuthResponseDto>> BecomeSellerAsync(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(CreateProblemDetails("Unauthorized.", "Invalid user token."));
            }

            try
            {
                var result = await _authService.BecomeSellerAsync(userId, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(CreateProblemDetails("User not found.", ex.Message));
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> MeAsync(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(CreateProblemDetails("Unauthorized.", "Invalid user token."));
            }

            var user = await _authService.GetUserByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return NotFound(CreateProblemDetails("User not found.", $"No user found with id {userId}."));
            }

            return Ok(user);
        }

        private void ClearAuthCookies()
        {
            var expiredAt = DateTimeOffset.UtcNow.AddDays(-1);
            var options = new CookieOptions
            {
                Expires = expiredAt,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };

            Response.Cookies.Append("access_token", string.Empty, options);
            Response.Cookies.Append("refresh_token", string.Empty, options);
        }

        private static ProblemDetails CreateProblemDetails(string title, string detail)
        {
            return new ProblemDetails
            {
                Title = title,
                Detail = detail
            };
        }
    }
}
