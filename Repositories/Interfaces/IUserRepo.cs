using Base.Api.Models;

namespace Base.Api.Repositories.Interfaces
{
    public interface IUserRepo
    {
        Task<(User user, List<string> permissions)> GetByUsernameAsync(string username);
        Task<List<string>> GetRolesByUserIdAsync(int userId);
        Task<int> UpdateLockoutAsync(Guid userGuid, int failedCount, DateTime? lastFailed, DateTime? lockedUntil);
        Task<int> UpdateUserAsync(User user);
        Task<int> AddAsync(User user);
    }
}
