using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNet.WebApi.Models
{
    public class Pedido
    {
        public int PedID { get; set; }
        public int PedUsu { get; set; }
        public Usuario Usuario { get; set; }
        public int PedPro { get; set; }
        public Producto Producto { get; set; }
        public decimal PedVrUnit { get; set; }
        public double PedCant { get; set; }
        public decimal PedSubTot { get; set; }
        public double PedIVA { get; set; }
        public decimal PedTotal { get; set; }
    }
}
