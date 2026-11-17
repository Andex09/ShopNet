using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNet.Models.Dtos
{
    public class UsuarioLoginRegisterDto
    {
        [Required]
        public string UsuNombre { get; set; }

        [Required]
        public string UsuPass { get; set; }
    }
}
