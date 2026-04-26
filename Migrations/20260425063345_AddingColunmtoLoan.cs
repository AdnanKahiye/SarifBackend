using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddingColunmtoLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "InterestRate",
                table: "Loans",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "Loans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Loans_CurrencyId",
                table: "Loans",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Currencies_CurrencyId",
                table: "Loans",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Currencies_CurrencyId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_CurrencyId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Loans");

            migrationBuilder.AlterColumn<decimal>(
                name: "InterestRate",
                table: "Loans",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
