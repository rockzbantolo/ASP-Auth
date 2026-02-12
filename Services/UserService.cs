using Base.Api.Dto.Request.Users;
using Base.Api.Dto.Response;
using Base.Api.Models;
using Base.Api.Repositories.Interfaces;
using Base.Api.Services.Interfaces;
using static Base.Api.Dto.Response.ResultBuilder;

namespace Base.Api.Services
{
    public class UserService(IUserRepo userRepo) : IUserService
    {
        private readonly IUserRepo _userRepo = userRepo;

        public async Task<Result<int>> AddAsync(CreateUserRequest request, UserSession userSession)
        {
            // Fetch existing user by username
            var (existingUser, _) = await _userRepo.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                return Fail<int>("Username already exists", 400);

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = request.Role,
                IsActive = request.IsActive,
                CreatedBy = userSession.Id,
                UpdatedBy = userSession.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserGuid = Guid.NewGuid() // ensure user_guid is generated
            };

            try
            {
                int userId = await _userRepo.AddAsync(user);
                return Success(userId);
            }
            catch (Exception ex)
            {
                return Fail<int>($"Failed to create user: {ex.Message}", 500);
            }
        }


        public async Task<Result<int>> UpdateUserAsync(UpdateUserRequest request, UserSession session)
        {
            // Fetch existing user by username
            var (existingUser, _) = await _userRepo.GetByUsernameAsync(request.Username);

            // If another user already has the same username
            if (existingUser != null && existingUser.UserGuid != request.UserGuid)
                return Fail<int>("Username already exists", 400);

            // Hash new password if provided
            string? passwordHash = null;
            if (!string.IsNullOrWhiteSpace(request.Password))
                passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                UserGuid = request.UserGuid,
                Username = request.Username,
                PasswordHash = passwordHash ?? string.Empty,
                Role = request.Role,
                IsActive = request.IsActive,
                UpdatedBy = session.Id,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                int rowsAffected = await _userRepo.UpdateUserAsync(user);
                if (rowsAffected == 0)
                    return Fail<int>("User not found", 404);

                return Success(rowsAffected);
            }
            catch (Exception ex)
            {
                return Fail<int>($"Failed to update user: {ex.Message}", 500);
            }
        }


        public Task<ResultResponse> DeleteAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<User>> GetByIdAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
