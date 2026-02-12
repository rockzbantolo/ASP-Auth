namespace Base.Api.Config
{
    public class RateLimitPolicyOptions
    {
        public int PermitLimit { get; set; } = 10;
        public int WindowMinutes { get; set; } = 1;
        public int QueueLimit { get; set; } = 2;
    }
}
