using Microsoft.EntityFrameworkCore;
using ShopNet.WebApi.Models;
using ShopNet.WebApi.Repositories.Contracts;

namespace ShopNet.WebApi.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ShopNetContext _context;

        public AuthRepository(ShopNetContext context)
        {
            _context = context;
        }
    
        public async Task<bool> ExisteUsuario(string nombre)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuNombre.ToLower() == nombre);
            return usuario != null;
        }

        public async Task<Usuario> Login(string nombre, string password)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuNombre == nombre);

            if (usuario == null)
                return null;

            if (!VerifyPasswordHash(password, usuario.UsuPassHash, usuario.UsuPassSalt))
                return null;

            return usuario;

        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA256(passwordSalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int idx = 0; idx < computeHash.Length; idx++)
                {
                    if (computeHash[idx] != passwordHash[idx])
                        return false;
                }

                return true;
            }
        }

        public async Task<Usuario> Register(Usuario usuario, string password)
        {
            byte[] passwordHash;
            byte[] passwordSalt;

            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            usuario.UsuPassHash = passwordHash;
            usuario.UsuPassSalt = passwordSalt;

            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA256())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
