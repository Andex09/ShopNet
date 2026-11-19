using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameBasicRoleToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Users SET Role = 'Customer' WHERE Role = 'Basic';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Users SET Role = 'Basic' WHERE Role = 'Customer';");
        }
    }
}
