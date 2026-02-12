namespace Base.Api.Dto.Request.Users
{
    public class UpdateUserRequest
    {
        public Guid UserGuid { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; }
    }
}
