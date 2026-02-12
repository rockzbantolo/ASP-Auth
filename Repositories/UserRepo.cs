using Base.Api.Data;
using Base.Api.Models;
using Base.Api.Repositories.Interfaces;
using Next.Clients.Providers;

namespace Base.Api.Repositories
{
    public class UserRepo(Connection connection) : IUserRepo
    {
        private readonly MySqlDb _db = connection.Db;

        // Fetch user info + permissions for RBAC
        public async Task<(User user, List<string> permissions)> GetByUsernameAsync(string username)
        {
            // Get user basic info including lockout fields
            var user = (await _db.QueryAsync<User>(@"
                SELECT 
                    user_id, 
                    user_guid, 
                    username, 
                    password_hash,
                    is_active,
                    failed_login_count,
                    last_failed_login,
                    locked_until
                FROM users
                WHERE username = @username
                LIMIT 1
            ", new { username })).FirstOrDefault();

            if (user == null)
                return (null, new List<string>());

            // Get all permissions for this user via roles
            var permissions = (await _db.QueryAsync<string>(@"
                SELECT p.permission_name
                FROM permissions p
                JOIN role_permissions rp ON rp.permission_id = p.permission_id
                JOIN user_roles ur ON ur.role_id = rp.role_id
                WHERE ur.user_id = @UserId",
                new { user.UserId })).ToList();

            return (user, permissions);
        }

        // Fetch roles for user
        public async Task<List<string>> GetRolesByUserIdAsync(int userId)
        {
            var roles = await _db.QueryAsync<string>(@"
                SELECT r.role_name
                FROM roles r
                JOIN user_roles ur ON ur.role_id = r.role_id
                WHERE ur.user_id = @UserId
            ", new { UserId = userId });

            return [.. roles];
        }

        public async Task<int> AddAsync(User user)
        {
            string insertStmt = @"
                INSERT INTO users (user_guid, username, password_hash, role, is_active, created_at, created_by, updated_at, updated_by)
                VALUES (@UserGuid, @Username, @PasswordHash, @Role, @IsActive, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy);";

            await _db.ExecuteAsync(
                insertStmt,
                new
                {
                    user.UserGuid,
                    user.Username,
                    user.PasswordHash,
                    user.Role,
                    user.IsActive,
                    user.CreatedAt,
                    user.CreatedBy,
                    user.UpdatedAt,
                    user.UpdatedBy
                }
            ).ConfigureAwait(false);

            int userId = await _db.ExecuteScalarAsync<int>(
                "SELECT LAST_INSERT_ID();"
            ).ConfigureAwait(false);

            return userId;
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            string updateStmt = @"
                    UPDATE users
                    SET username = @Username,
                        password_hash = @PasswordHash,
                        role = @Role,
                        is_active = @IsActive
                    WHERE user_guid = @UserGuid;";

            int rowsAffected = await _db.ExecuteAsync(
                updateStmt,
                new
                {
                    user.Username,
                    user.PasswordHash,
                    user.Role,
                    user.IsActive,
                    user.UserGuid
                }
            ).ConfigureAwait(false);

            return rowsAffected;
        }

        public async Task<int> UpdateLockoutAsync(Guid userGuid, int failedCount, DateTime? lastFailed, DateTime? lockedUntil)
        {
            string updateStmt = @"
                    UPDATE users
                    SET failed_login_count = @FailedLoginCount,
                        last_failed_login = @LastFailedLogin,
                        locked_until = @LockedUntil
                    WHERE user_guid = @UserGuid;";

            return await _db.ExecuteAsync(updateStmt, new
            {
                FailedLoginCount = failedCount,
                LastFailedLogin = lastFailed,
                LockedUntil = lockedUntil,
                UserGuid = userGuid
            });
        }
    }
}
