namespace Base.Api.Dto.Request.Users
{
    public class ChangePassRequest
    {
        public Guid UserGuid { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
