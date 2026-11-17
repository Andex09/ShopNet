using ShopNet.WebApi.Models;

namespace ShopNet.WebApi.Repositories.Contracts
{
    public interface IAuthRepository
    {
        Task<Usuario> Register(Usuario usuario, string password);
        Task<Usuario> Login(string nombre, string password);
        Task<bool> ExisteUsuario(string nombre);
    }
}
