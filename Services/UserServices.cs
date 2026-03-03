using Base.Api.Dto.Request.Users;
using Base.Api.Dto.Response;
using Base.Api.Models;
using Base.Api.Repositories.Interfaces;
using Base.Api.Services.Interfaces;
using static Base.Api.Dto.Response.ResultBuilder;

namespace Base.Api.Services
{
    public class UserServices(IUserRepository userRepo) : IUserService
    {
        private readonly IUserRepository _userRepo = userRepo;

        public async Task<Result<int>> AddAsync(CreateUserRequest request, SessionResponse session)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Fail<int>("Username and password required", 400);

            var (existing, _, _) = await _userRepo.GetByUsernameAsync(request.Username);
            if (existing != null)
                return Fail<int>("Username already exists", 409);

            var user = new UserModel
            {
                UserGuid = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = request.IsActive,
                CreatedBy = session.Id,
                UpdatedBy = session.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            int userId = await _userRepo.AddAsync(user);
            return Success(userId);
        }

        public async Task<Result<int>> UpdateUserAsync(UpdateUserRequest request, SessionResponse session)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return Fail<int>("Username required", 400);

            var (existing, _, _) = await _userRepo.GetByUsernameAsync(request.Username);
            if (existing != null && existing.UserGuid != request.UserGuid)
                return Fail<int>("Username already exists", 409);

            string? newHash = null;
            if (!string.IsNullOrWhiteSpace(request.Password))
                newHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new UserModel
            {
                UserGuid = request.UserGuid,
                Username = request.Username,
                PasswordHash = newHash,           // null = keep existing
                IsActive = request.IsActive,
                UpdatedBy = session.Id,
                UpdatedAt = DateTime.UtcNow
            };

            int rows = await _userRepo.UpdateUserAsync(user);
            return rows > 0 ? Success(rows) : Fail<int>("User not found", 404);
        }

        public Task<ResultResponse> DeleteAsync(int userId) => throw new NotImplementedException();
        public Task<Result<UserModel>> GetByIdAsync(int userId) => throw new NotImplementedException();
    }

}
