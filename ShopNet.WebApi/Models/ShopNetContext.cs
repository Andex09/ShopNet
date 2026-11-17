using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNet.WebApi.Models
{
    public class ShopNetContext: DbContext
    {
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }

        public ShopNetContext(DbContextOptions<ShopNetContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            List<Producto> productoInit = new List<Producto>
            {
                new Producto()
                {
                    ProdID = 1,
                    ProDesc = "Platzi",
                    ProValor = 400000
                },
                new Producto()
                {
                    ProdID = 2,
                    ProDesc = "Mx 3 Mouse",
                    ProValor = 350000
                },
                new Producto()
                {
                    ProdID = 3,
                    ProDesc = "Audifonos Logitech",
                    ProValor = 200000
                },

            };

            modelBuilder.Entity<Producto>(producto =>
            {
                producto.ToTable("Productos");
                producto.HasKey(p => p.ProdID);
                producto.Property(p => p.ProDesc).HasMaxLength(50);
                producto.HasData(productoInit);
            });


            modelBuilder.Entity<Usuario>(usuario =>
            {
                usuario.ToTable("Usuarios");
                usuario.HasKey(p => p.UsuID);
                usuario.HasIndex(p => p.UsuNombre).IsUnique();
                usuario.Property(p => p.UsuNombre).HasMaxLength(50);
            });

            modelBuilder.Entity<Pedido>(pedido =>
            {
                pedido.ToTable("Pedidos");
                pedido.HasKey(p => p.PedID);
                pedido.HasOne(p => p.Usuario).WithMany(b => b.Pedidos).HasForeignKey(p => p.PedUsu);
                pedido.HasOne(p => p.Producto).WithMany(b => b.Pedidos).HasForeignKey(p => p.PedPro);
            });
        }
    }
}
