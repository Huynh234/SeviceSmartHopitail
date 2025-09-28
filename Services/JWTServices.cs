using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SeviceSmartHopitail.Services
{
    public class JWTServices
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JWTServices(IConfiguration configuration)
        {
            _secretKey = configuration["JwtSettings:SecretKey"] ?? "vu_dinh_huynh_vu_dinh_huynh_vu_dinh_huynh";
            _issuer = configuration["JwtSettings:Issuer"] ?? "user";
            _audience = configuration["JwtSettings:Audience"] ?? "user";
        }
        public string GenerateToken(string username, string role = "user")
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claim = new[]
            {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            };
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claim,
                expires: DateTime.Now.AddMinutes(60 * 2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
