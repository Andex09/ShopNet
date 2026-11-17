using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNet.WebApi.Migrations
{
    public partial class CreacionDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    ProdID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProDesc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProValor = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.ProdID);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuNombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsuRol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuPassHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UsuPassSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuID);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    PedID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedUsu = table.Column<int>(type: "int", nullable: false),
                    PedPro = table.Column<int>(type: "int", nullable: false),
                    PedVrUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PedCant = table.Column<double>(type: "float", nullable: false),
                    PedSubTot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PedIVA = table.Column<double>(type: "float", nullable: false),
                    PedTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.PedID);
                    table.ForeignKey(
                        name: "FK_Pedidos_Productos_PedPro",
                        column: x => x.PedPro,
                        principalTable: "Productos",
                        principalColumn: "ProdID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedidos_Usuarios_PedUsu",
                        column: x => x.PedUsu,
                        principalTable: "Usuarios",
                        principalColumn: "UsuID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "ProdID", "ProDesc", "ProValor" },
                values: new object[] { 1, "Platzi", 400000m });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "ProdID", "ProDesc", "ProValor" },
                values: new object[] { 2, "Mx 3 Mouse", 350000m });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "ProdID", "ProDesc", "ProValor" },
                values: new object[] { 3, "Audifonos Logitech", 200000m });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_PedPro",
                table: "Pedidos",
                column: "PedPro");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_PedUsu",
                table: "Pedidos",
                column: "PedUsu");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UsuNombre",
                table: "Usuarios",
                column: "UsuNombre",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
