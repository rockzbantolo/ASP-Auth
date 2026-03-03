namespace Base.Api.Models
{
    public class AuthSession
    {
        public Guid SessionId { get; set; }
        public int UserId { get; set; }
        public string RefreshTokenHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime AccessExpiresAt { get; set; }
        public DateTime RefreshExpiresAt { get; set; }
        public string? LastIp { get; set; }
        public string? UserAgent { get; set; }
        public List<string> Roles { get; set; } = new();
        public HashSet<string> Permissions { get; set; } = new();
    }
}
