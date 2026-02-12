using Base.Api.Dto.Request;
using Base.Api.Dto.Response;
using Base.Api.Helpers;
using Base.Api.Repositories.Interfaces;
using Base.Api.Services.Interfaces;
using static Base.Api.Dto.Response.ResultBuilder;
using System.Diagnostics;

namespace Base.Api.Services
{
    public class AuthService(IUserRepo userRepo, JwtHelper jwtHelper) : IAuthService
    {
        private readonly IUserRepo _userRepo = userRepo;
        private readonly JwtHelper _jwtHelper = jwtHelper;

        public async Task<Result<UserSession>> LoginAsync(AuthRequest request)
        {
            // 1. Fetch user and permissions from DB
            var (user, permissions) = await _userRepo.GetByUsernameAsync(request.Username);
            if (user == null)
                return Fail<UserSession>("Incorrect username or password", 401);

            // 2. Check if account is locked
            if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            {
                return Fail<UserSession>(
                    $"Account is locked until {user.LockedUntil.Value:yyyy-MM-dd HH:mm:ss}",
                    403
                );
            }

            // 3. Verify password
            bool validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!validPassword)
            {
                // Increment failed login count
                user.FailedLoginCount += 1;
                user.LastFailedLogin = DateTime.UtcNow;

                int maxAttempts = 5;               // configurable
                int lockMinutes = 15;              // configurable

                if (user.FailedLoginCount >= maxAttempts)
                {
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(lockMinutes);
                    user.FailedLoginCount = 0;    // reset after lock
                }

                // Update lockout/failure fields in DB
                await _userRepo.UpdateLockoutAsync(
                    user.UserGuid,
                    user.FailedLoginCount,
                    user.LastFailedLogin,
                    user.LockedUntil
                );

                return Fail<UserSession>("Incorrect username or password", 401);
            }

            // 4. Successful login: reset lockout counters
            user.FailedLoginCount = 0;
            user.LockedUntil = null;
            user.LastFailedLogin = null;

            await _userRepo.UpdateLockoutAsync(
                user.UserGuid,
                0,
                null,
                null
            );

            // 5. Fetch roles for RBAC
            var roles = await _userRepo.GetRolesByUserIdAsync(user.UserId);
            if (roles == null || roles.Count == 0)
                roles = [user.Role ?? "User"];

            // 6. Generate JWT including roles and permissions
            string token = _jwtHelper.GenerateToken(
                userId: user.UserId,
                username: user.Username,
                roles: roles,
                permissions: permissions
            );

            // 7. Build user session
            var session = new UserSession
            {
                Id = user.UserId,
                UserGuid = user.UserGuid,
                Username = user.Username,
                Role = string.Join(",", roles), // store all roles in one string if multiple
                Token = token
            };

            return Success(session);
        }

    }
}
