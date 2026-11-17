using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNet.WebApi.Models
{
    public class Usuario
    {
        public int UsuID { get; set; }
        public string UsuNombre { get; set; }
        public string UsuRol { get; set; }
        public byte[] UsuPassHash { get; set; }
        public byte[] UsuPassSalt { get; set; }
        public List<Pedido> Pedidos { get; set; }

    }
}
