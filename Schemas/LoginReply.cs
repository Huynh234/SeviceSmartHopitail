using System.Runtime.CompilerServices;

namespace SeviceSmartHopitail.Schemas
{
    public class LoginReply
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; } = "@@##**";
        public string? Role { get; set; }

        public string? Token { get; set; }
    }
}
