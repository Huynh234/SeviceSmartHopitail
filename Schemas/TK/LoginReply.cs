using SeviceSmartHopitail.Models;
using System.Runtime.CompilerServices;

namespace SeviceSmartHopitail.Schemas.TK
{
    public class LoginReply
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; } = "@@##**";
        public string? Role { get; set; }
        public bool check { get; set; } = true;

        public int? Pf { get; set; }

    }
}
