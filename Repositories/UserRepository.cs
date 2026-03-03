using Base.Api.Data;
using Base.Api.Models;
using Base.Api.Repositories.Interfaces;
using Next.Clients.Providers;

namespace Base.Api.Repositories
{
    public class UserRepository(Connection connection) : IUserRepository
    {
        private readonly MySqlDb _db = connection.Db;

        public async Task<(UserModel? user, List<string> roles, List<string> permissions)> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return (null, [], []);

            const string sql = @"
                SELECT 
                    u.user_id, u.user_guid, u.username, u.password_hash, u.is_active, u.is_online,
                    u.failed_login_count, u.last_failed_login, u.locked_until,
                    GROUP_CONCAT(DISTINCT r.role_name) AS roles,
                    GROUP_CONCAT(DISTINCT p.permission_name) AS permissions
                FROM users u
                LEFT JOIN user_roles ur ON ur.user_id = u.user_id
                LEFT JOIN roles r ON r.role_id = ur.role_id
                LEFT JOIN role_permissions rp ON rp.role_id = r.role_id
                LEFT JOIN permissions p ON p.permission_id = rp.permission_id
                WHERE u.username = @username
                GROUP BY u.user_id
                LIMIT 1";

            var row = (await _db.QueryAsync<dynamic>(sql, new { username })).FirstOrDefault();
            if (row == null) return (null, [], []);

            var user = new UserModel
            {
                UserId              = row.user_id,
                UserGuid            = row.user_guid,
                Username            = row.username,
                PasswordHash        = row.password_hash,
                IsOnline            = row.is_online,
                FailedLoginCount    = row.failed_login_count,
                LastFailedLogin     = row.last_failed_login,
                LockedUntil         = row.locked_until
            };

            var rolesList = row.roles is string r && !string.IsNullOrEmpty(r)
                ? r.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : [];

            var permsList = row.permissions is string p && !string.IsNullOrEmpty(p)
                ? p.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : [];

            return (user, rolesList, permsList);
        }

        public async Task<int> AddAsync(UserModel user)
        {
            const string sql = @"
                INSERT INTO users (
                    user_guid, username, password_hash, is_active, 
                    created_at, created_by, updated_at, updated_by
                ) VALUES (
                    @UserGuid, @Username, @PasswordHash, @IsActive,
                    @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy
                ); SELECT LAST_INSERT_ID();";

            return await _db.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<int> UpdateUserAsync(UserModel user)
        {
            const string sql = @"
                UPDATE users SET
                    username      = @Username,
                    password_hash = COALESCE(@PasswordHash, password_hash),
                    is_active     = @IsActive,
                    updated_at    = @UpdatedAt,
                    updated_by    = @UpdatedBy
                WHERE user_guid = @UserGuid";

            return await _db.ExecuteAsync(sql, new
            {
                user.Username,
                user.PasswordHash,
                user.IsActive,
                UpdatedAt = DateTime.UtcNow,
                user.UpdatedBy,
                user.UserGuid
            });
        }

        public Task<int> UpdateLockoutAsync(Guid userGuid, int failedCount, DateTime? lastFailed, DateTime? lockedUntil)
        {
            return _db.ExecuteAsync(@"
                UPDATE users SET
                    failed_login_count = @FailedLoginCount,
                    last_failed_login  = @LastFailedLogin,
                    locked_until       = @LockedUntil
                WHERE user_guid = @UserGuid",
                new { FailedLoginCount = failedCount, LastFailedLogin = lastFailed, LockedUntil = lockedUntil, UserGuid = userGuid });
        }

        public Task SetOnlineStatusAsync(Guid userGuid, bool isOnline)
        {
            return _db.ExecuteAsync(
                "UPDATE users SET is_online = @IsOnline WHERE user_guid = @UserGuid",
                new { IsOnline = isOnline, UserGuid = userGuid });
        }

        public async Task<UserAuthContext?> GetUserAuthenticationAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            const string sql = @"
            SELECT 
                u.user_id           AS UserId,
                u.user_guid         AS UserGuid,
                u.username          AS Username,
                u.password_hash     AS PasswordHash,
                u.is_active         AS IsActive,
                u.is_online         AS IsOnline,
                u.failed_login_count AS FailedLoginCount,
                u.last_failed_login AS LastFailedLogin,
                u.locked_until      AS LockedUntil,

                -- Distinct roles
                (SELECT GROUP_CONCAT(DISTINCT r.role_name ORDER BY r.role_name)
                    FROM user_roles ur2
                    JOIN roles r ON r.role_id = ur2.role_id
                    WHERE ur2.user_id = u.user_id) AS RolesJson,

                -- Distinct permissions
                (SELECT GROUP_CONCAT(DISTINCT p.permission_name ORDER BY p.permission_name)
                    FROM user_roles ur2
                    JOIN roles r ON r.role_id = ur2.role_id
                    JOIN role_permissions rp ON rp.role_id = r.role_id
                    JOIN permissions p ON p.permission_id = rp.permission_id
                    WHERE ur2.user_id = u.user_id) AS PermissionsJson
            FROM users u
            WHERE u.username = @username
                AND u.is_active = 1
            LIMIT 1";

            var row = await _db.QueryFirstOrDefaultAsync<dynamic>(sql, new { username });
            if (row == null) return null;

            var roles = row.RolesJson is string r && !string.IsNullOrEmpty(r)
                ? r.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : new List<string>();

            var perms = row.PermissionsJson is string p && !string.IsNullOrEmpty(p)
                ? p.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : Array.Empty<string>();

            return new UserAuthContext
            {
                UserId = row.UserId,
                UserGuid = row.UserGuid,
                Username = row.Username,
                PasswordHash = row.PasswordHash,
                IsActive = row.IsActive,
                IsOnline = row.IsOnline,
                FailedLoginCount = row.FailedLoginCount,
                LastFailedLogin = row.LastFailedLogin,
                LockedUntil = row.LockedUntil,
                Roles = roles.AsReadOnly(),
                Permissions = new HashSet<string>(perms)
            };
        }
    
    }
}
