using Base.Api.Dto.Request;
using Base.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Base.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [AllowAnonymous]
        [HttpPost("Login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                return StatusCode(result.Code, result);

            return Ok(result);
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userGuid = User.FindFirstValue("UserGuid");
            if (string.IsNullOrEmpty(userGuid) || !Guid.TryParse(userGuid, out var guid))
                return Unauthorized();

            var result = await _authService.LogoutAsync(guid);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("Test-Auth")]
        public IActionResult TestAuth()
        {
            var username = User.Identity?.Name ?? "Unknown";
            var userId = User.FindFirst("UserId")?.Value ?? "None";

            return Ok(new
            {
                Message = "Protected endpoint accessed",
                Username = username,
                UserId = userId
            });
        }
    }
}
