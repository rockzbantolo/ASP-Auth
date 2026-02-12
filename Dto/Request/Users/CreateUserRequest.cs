namespace Base.Api.Dto.Request.Users
{
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;

        // Required raw password; service will hash before saving
        public string Password { get; set; } = string.Empty;

        // Optional role; defaults to "User"
        public string Role { get; set; } = "User";

        // Optional active status; default to true
        public bool IsActive { get; set; } = true;
    }
}
