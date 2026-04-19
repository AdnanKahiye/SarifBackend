using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CashAccountId",
                table: "LoanPayments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LoanAccountId",
                table: "LoanPayments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ExchangeDto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false),
                    FromAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ToAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Fee = table.Column<decimal>(type: "numeric", nullable: false),
                    Profit = table.Column<decimal>(type: "numeric", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromAccountName = table.Column<string>(type: "text", nullable: true),
                    ToAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToAccountName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeDto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeDto_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExchangeDto_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayments_CashAccountId",
                table: "LoanPayments",
                column: "CashAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayments_LoanAccountId",
                table: "LoanPayments",
                column: "LoanAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeDto_AgencyId",
                table: "ExchangeDto",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeDto_BranchId",
                table: "ExchangeDto",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanPayments_Accounts_CashAccountId",
                table: "LoanPayments",
                column: "CashAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanPayments_Accounts_LoanAccountId",
                table: "LoanPayments",
                column: "LoanAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanPayments_Accounts_CashAccountId",
                table: "LoanPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanPayments_Accounts_LoanAccountId",
                table: "LoanPayments");

            migrationBuilder.DropTable(
                name: "ExchangeDto");

            migrationBuilder.DropIndex(
                name: "IX_LoanPayments_CashAccountId",
                table: "LoanPayments");

            migrationBuilder.DropIndex(
                name: "IX_LoanPayments_LoanAccountId",
                table: "LoanPayments");

            migrationBuilder.DropColumn(
                name: "CashAccountId",
                table: "LoanPayments");

            migrationBuilder.DropColumn(
                name: "LoanAccountId",
                table: "LoanPayments");
        }
    }
}
