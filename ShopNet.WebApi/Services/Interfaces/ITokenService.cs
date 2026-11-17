using ShopNet.WebApi.Models;

namespace ShopNet.WebApi.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(Usuario usuario);
        
    }
}
