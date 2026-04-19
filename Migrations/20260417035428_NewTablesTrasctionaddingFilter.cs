using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class NewTablesTrasctionaddingFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AgencyId",
                table: "Withdraws",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AgencyId",
                table: "Transfers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Transfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AgencyId",
                table: "LoanPayments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AgencyId",
                table: "Exchanges",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Exchanges",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AgencyId",
                table: "Deposits",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Withdraws_AgencyId",
                table: "Withdraws",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdraws_BranchId",
                table: "Withdraws",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_AgencyId",
                table: "Transfers",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_BranchId",
                table: "Transfers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayments_AgencyId",
                table: "LoanPayments",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayments_BranchId",
                table: "LoanPayments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_AgencyId",
                table: "Exchanges",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_BranchId",
                table: "Exchanges",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_AgencyId",
                table: "Deposits",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_BranchId",
                table: "Deposits",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deposits_Agencies_AgencyId",
                table: "Deposits",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deposits_Branches_BranchId",
                table: "Deposits",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Exchanges_Agencies_AgencyId",
                table: "Exchanges",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exchanges_Branches_BranchId",
                table: "Exchanges",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanPayments_Agencies_AgencyId",
                table: "LoanPayments",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanPayments_Branches_BranchId",
                table: "LoanPayments",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Agencies_AgencyId",
                table: "Transfers",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Branches_BranchId",
                table: "Transfers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Withdraws_Agencies_AgencyId",
                table: "Withdraws",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Withdraws_Branches_BranchId",
                table: "Withdraws",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deposits_Agencies_AgencyId",
                table: "Deposits");

            migrationBuilder.DropForeignKey(
                name: "FK_Deposits_Branches_BranchId",
                table: "Deposits");

            migrationBuilder.DropForeignKey(
                name: "FK_Exchanges_Agencies_AgencyId",
                table: "Exchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_Exchanges_Branches_BranchId",
                table: "Exchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanPayments_Agencies_AgencyId",
                table: "LoanPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanPayments_Branches_BranchId",
                table: "LoanPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Agencies_AgencyId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Branches_BranchId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Withdraws_Agencies_AgencyId",
                table: "Withdraws");

            migrationBuilder.DropForeignKey(
                name: "FK_Withdraws_Branches_BranchId",
                table: "Withdraws");

            migrationBuilder.DropIndex(
                name: "IX_Withdraws_AgencyId",
                table: "Withdraws");

            migrationBuilder.DropIndex(
                name: "IX_Withdraws_BranchId",
                table: "Withdraws");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_AgencyId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_BranchId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_LoanPayments_AgencyId",
                table: "LoanPayments");

            migrationBuilder.DropIndex(
                name: "IX_LoanPayments_BranchId",
                table: "LoanPayments");

            migrationBuilder.DropIndex(
                name: "IX_Exchanges_AgencyId",
                table: "Exchanges");

            migrationBuilder.DropIndex(
                name: "IX_Exchanges_BranchId",
                table: "Exchanges");

            migrationBuilder.DropIndex(
                name: "IX_Deposits_AgencyId",
                table: "Deposits");

            migrationBuilder.DropIndex(
                name: "IX_Deposits_BranchId",
                table: "Deposits");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "LoanPayments");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "Exchanges");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Exchanges");

            migrationBuilder.AlterColumn<Guid>(
                name: "AgencyId",
                table: "Withdraws",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "AgencyId",
                table: "Deposits",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
