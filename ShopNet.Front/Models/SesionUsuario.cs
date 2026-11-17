using ShopNet.Models.Dtos;

namespace ShopNet.Front.Models
{
    public class SesionUsuario
    {
        public string token { get; set; }
        public UsuarioListDto usuario { get; set; }
    }
}
