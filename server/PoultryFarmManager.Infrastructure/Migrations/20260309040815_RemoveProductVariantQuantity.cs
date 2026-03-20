using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoultryFarmManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductVariantQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ProductVariants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ProductVariants",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }
    }
}
