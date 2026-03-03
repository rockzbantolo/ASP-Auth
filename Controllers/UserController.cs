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

        private SessionResponse BuildSession()
        {
            return new SessionResponse
            {
                Id = int.Parse(User.FindFirst("id")!.Value),
                Username = User.Identity!.Name!,
                Role = User.FindFirst("role")!.Value,
                Token = ""
            };
        }

        [HttpPost]
        [Authorize(Roles = "System")]
        public async Task<IActionResult> Add([FromBody] CreateUserRequest request)
        {
            var session = BuildSession();

            var result = await _userService.AddAsync(request, session);

            if (!result.Success)
                return StatusCode(result.Code, result);

            return Ok(result);
        }

        [HttpPut("{userGuid:guid}")]
        [Authorize(Roles = "System")]
        public async Task<IActionResult> Update(Guid userGuid, [FromBody] UpdateUserRequest request)
        {
            request.UserGuid = userGuid;

            var session = BuildSession();

            var result = await _userService.UpdateUserAsync(request, session);

            if (!result.Success)
                return StatusCode(result.Code, result);

            return Ok(result);
        }

        [HttpGet("Error")]
        public IActionResult TestError()
        {
            throw new Exception("Error Response Testing");
        }
    }

}
