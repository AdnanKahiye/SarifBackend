using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class NewCoulmToExchangeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "Exchanges",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FromCurrencyId",
                table: "ExchangeDto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToCurrencyId",
                table: "ExchangeDto",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "Exchanges");

            migrationBuilder.DropColumn(
                name: "FromCurrencyId",
                table: "ExchangeDto");

            migrationBuilder.DropColumn(
                name: "ToCurrencyId",
                table: "ExchangeDto");
        }
    }
}
