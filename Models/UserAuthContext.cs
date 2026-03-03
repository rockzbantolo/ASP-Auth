namespace Base.Api.Models
{
    public class UserAuthContext
    {
        public required int UserId { get; init; }
        public required Guid UserGuid { get; init; }
        public required string Username { get; init; }
        public required string PasswordHash { get; init; }
        public required bool IsActive { get; init; }
        public required bool IsOnline { get; init; }
        public required int FailedLoginCount { get; init; }
        public DateTime? LastFailedLogin { get; init; }
        public DateTime? LockedUntil { get; init; }

        public required IReadOnlyList<string> Roles { get; init; }
        public required IReadOnlySet<string> Permissions { get; init; }  // HashSet for O(1) lookup

        // Optional future extensions (no breaking change)
        // public DateTime? LastLogin { get; init; }
    }
}
