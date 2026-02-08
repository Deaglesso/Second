using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
