using Microsoft.EntityFrameworkCore;
using ShopNet.WebApi.Models;
using ShopNet.WebApi.Repositories.Contracts;

namespace ShopNet.WebApi.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ShopNetContext _context;

        public UsuarioRepository(ShopNetContext context)
        {
            _context = context;
        }

        public async Task<Usuario> AddItem(Usuario usuario, string password)
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

        public async Task DeleteItem(Usuario usuario)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario> GetItem(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<List<Usuario>> GetItems()
        {
            return await _context.Usuarios.ToListAsync();

        }

        public async Task UpdateItem(Usuario usuario, string password)
        {
            var usuarioItem = await _context.Usuarios.FirstOrDefaultAsync(p => p.UsuID == usuario.UsuID);

            if (usuarioItem != null)
            {
                usuarioItem.UsuNombre = usuario.UsuNombre;
                usuarioItem.UsuRol = usuario.UsuRol;

                if (!string.IsNullOrEmpty(password))
                {
                    byte[] passwordHash;
                    byte[] passwordSalt;

                    CreatePasswordHash(password, out passwordHash, out passwordSalt);

                    usuarioItem.UsuPassHash = passwordHash;
                    usuarioItem.UsuPassSalt = passwordSalt;
                }

                _context.Update(usuarioItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteUsuario(string nombre)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuNombre.ToLower() == nombre);
            return usuario != null;
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
