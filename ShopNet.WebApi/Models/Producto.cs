using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNet.WebApi.Models
{
    public class Producto
    {
        public int ProdID { get; set; }
        public string ProDesc { get; set; }
        public decimal ProValor { get; set; }

        public List<Pedido> Pedidos { get; set; }

    }
}
