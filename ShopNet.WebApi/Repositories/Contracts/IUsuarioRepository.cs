using ShopNet.WebApi.Models;

namespace ShopNet.WebApi.Repositories.Contracts
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetItems();
        Task<Usuario> GetItem(int id);
        Task DeleteItem(Usuario usuario);
        Task<Usuario> AddItem(Usuario usuario, string password);
        Task UpdateItem(Usuario usuario, string password);

        Task<bool> ExisteUsuario(string nombre);
    }
}
