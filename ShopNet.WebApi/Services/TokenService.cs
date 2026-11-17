using Microsoft.IdentityModel.Tokens;
using ShopNet.WebApi.Models;
using ShopNet.WebApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShopNet.WebApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _ssKey;
        public TokenService(IConfiguration config)
        {
            _ssKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(config["Token"]));
        }
        public string CreateToken(Usuario usuario)
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.UsuNombre),
                new Claim(ClaimTypes.Role, usuario.UsuRol)
            };

            var credenciales = new SigningCredentials(_ssKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credenciales
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
