using Base.Api.Dto.Request.Users;
using Base.Api.Dto.Response;
using Base.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Mozilla;

namespace Base.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost]
        [Authorize(Roles = "SystemAdmin")]// optional, only admins can create users
        public async Task<IActionResult> Add([FromBody] CreateUserRequest request)
        {
            // Build session from JWT claims
            var userSession = new UserSession
            {
                Id = int.Parse(User.FindFirst("id")!.Value),
                Username = User.Identity!.Name!,
                Role = User.FindFirst("role")!.Value,
                Token = "" // optional
            };

            var result = await _userService.AddAsync(request, userSession);
            if (!result.Success)
                return StatusCode(result.Code ?? 400, result);

            return Ok(result); // Returns new user ID on success
        }

        [HttpPut("{userGuid:guid}")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Update(Guid userGuid, [FromBody] UpdateUserRequest request)
        {
            // Set the userGuid from route
            request.UserGuid = userGuid;

            var userSession = new UserSession
            {
                Id = int.Parse(User.FindFirst("id")!.Value),
                Username = User.Identity!.Name!,
                Role = User.FindFirst("role")!.Value,
                Token = ""
            };

            var result = await _userService.UpdateUserAsync(request, userSession);
            if (!result.Success)
                return StatusCode(result.Code ?? 400, result);

            return Ok(result);
        }

    }
}
