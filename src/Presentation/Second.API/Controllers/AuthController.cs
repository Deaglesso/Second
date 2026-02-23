using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Second.Application.Contracts.Services;
using Second.Application.Dtos;
using Second.Application.Dtos.Requests;
using Second.Application.Exceptions;

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
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponseDto>> RegisterAsync([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponseDto>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
        {
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

            if (string.IsNullOrWhiteSpace(jti) || string.IsNullOrWhiteSpace(expClaim) || !long.TryParse(expClaim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var exp))
            {
                throw new UnauthorizedAppException("Token metadata is missing.", "token_metadata_missing");
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
                throw new BadRequestAppException("The token is invalid or expired.", "invalid_email_verification_token");
            }

            return Ok(new { Message = "Email verified successfully." });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            await _authService.RequestPasswordResetAsync(request, cancellationToken);
            return Ok(new { Message = "If an account exists, a reset link has been sent." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            await _authService.ResetPasswordAsync(request, cancellationToken);
            ClearAuthCookies();
            return Ok(new { Message = "Password reset successfully." });
        }

        [HttpPost("become-seller")]
        [Authorize]
        public async Task<ActionResult<AuthResponseDto>> BecomeSellerAsync(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAppException("Invalid user token.", "invalid_user_token");
            }

            var result = await _authService.BecomeSellerAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> MeAsync(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAppException("Invalid user token.", "invalid_user_token");
            }

            var user = await _authService.GetUserByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                throw new NotFoundAppException($"No user found with id {userId}.", "user_not_found");
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
    }
}
