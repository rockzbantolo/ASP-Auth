using Base.Api.Dto.Request;
using Base.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Base.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        /// <summary>
        /// Login user and return JWT token
        /// </summary>
        [AllowAnonymous] // Must allow unauthenticated access
        [HttpPost("Login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                return StatusCode(result.Code ?? 400, result); // 401 for invalid credentials, etc.

            return Ok(result); // Success with 200
        }

        /// <summary>
        /// Test endpoint to verify JWT access
        /// </summary>
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
