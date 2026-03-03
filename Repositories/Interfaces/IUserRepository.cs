using Base.Api.Models;

namespace Base.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<(UserModel? user, List<string> roles, List<string> permissions)> GetByUsernameAsync(string username);

        Task<UserAuthContext?> GetUserAuthenticationAsync(string username);

        Task<int> AddAsync(UserModel user);

        Task<int> UpdateUserAsync(UserModel user);

        Task<int> UpdateLockoutAsync(Guid userGuid, int failedCount, DateTime? lastFailed, DateTime? lockedUntil);

        Task SetOnlineStatusAsync(Guid userGuid, bool isOnline);
    }
}
