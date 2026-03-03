using Base.Api.Dto.Request;
using Base.Api.Dto.Response;
using Base.Api.Helpers;
using Base.Api.Models;
using Base.Api.Repositories.Interfaces;
using Base.Api.Services.Interfaces;
using Microsoft.AspNetCore.Session;
using MySqlX.XDevAPI.Common;
using System.Diagnostics;
using static Base.Api.Dto.Response.ResultBuilder;

namespace Base.Api.Services
{
    public class AuthServices(IUserRepository userRepo, JwtHelper jwtHelper) : IAuthService
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly JwtHelper _jwtHelper = jwtHelper;

        public async Task<Result<SessionResponse>> LoginAsync(AuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
                return Fail<SessionResponse>("Invalid credentials", 400);

            var (user, roles, permissions) = await _userRepo.GetByUsernameAsync(request.Username);

            if (user == null)
                return Fail<SessionResponse>("Invalid credentials", 401);

            if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
                return Fail<SessionResponse>($"Locked until {user.LockedUntil.Value:yyyy-MM-dd HH:mm:ss}", 403);

            Debug.WriteLine($"Login Status: {user.IsOnline}");

            if (user.IsOnline)
                return Fail<SessionResponse>("User is already logged in on another device", 409);

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await UpdateFailedLogin(user);
                return Fail<SessionResponse>("Invalid credentials", 401);
            }

            await ResetLockoutIfNeeded(user);

            // Build Token
            var token = _jwtHelper.GenerateToken(
                user.UserId,
                user.UserGuid,
                user.Username,
                roles,
                permissions
            );

            // Build Session
            var session = new SessionResponse
            {
                Id = user.UserId,
                UserGuid = user.UserGuid,
                Username = user.Username,
                Role = string.Join(",", roles),
                Token = token
            };

            // Set Status Online
            await _userRepo.SetOnlineStatusAsync(user.UserGuid, true);
            return Success(session);
        }

        private async Task UpdateFailedLogin(UserModel user)
        {
            user.FailedLoginCount++;
            user.LastFailedLogin = DateTime.UtcNow;

            if (user.FailedLoginCount >= 5)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginCount = 0;
            }

            await _userRepo.UpdateLockoutAsync(user.UserGuid, user.FailedLoginCount, user.LastFailedLogin, user.LockedUntil);
        }

        private async Task ResetLockoutIfNeeded(UserModel user)
        {
            if (user.FailedLoginCount > 0 || user.LockedUntil.HasValue)
                await _userRepo.UpdateLockoutAsync(user.UserGuid, 0, null, null);
        }

        public async Task<ResultResponse> LogoutAsync(Guid userGuid)
        {
            await _userRepo.SetOnlineStatusAsync(userGuid, false);
            return Success(message: "Logged out");
        }
    }
}
