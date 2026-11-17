using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNet.Models.Dtos
{
    public class ProductoDto
    {
        public int ProdID { get; set; }
        [Required]
        public string ProDesc { get; set; }
        
        [Required]
        public decimal ProValor { get; set; }
    }
}
