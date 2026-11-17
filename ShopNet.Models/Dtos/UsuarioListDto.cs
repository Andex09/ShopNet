using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNet.Models.Dtos
{
    public class UsuarioListDto
    {
        public int UsuID { get; set; }
        public string UsuNombre { get; set; }
        public string UsuRol { get; set; }
    }
}
