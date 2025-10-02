
namespace AuthApp.Config
{

    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsUsed { get; set; } = false;
        public bool IsRevoked { get; set; } = false;
    }
}
