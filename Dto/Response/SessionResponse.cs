namespace Base.Api.Dto.Response
{
    public class SessionResponse
    {
        public int Id { get; set; }
        public Guid UserGuid { get; set; }
        public required string Username { get; set; } 
        public  required string Role { get; set; }  
        public required string Token { get; set; } 
    }
}
