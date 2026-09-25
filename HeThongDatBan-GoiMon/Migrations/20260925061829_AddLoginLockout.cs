using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongDatBan_GoiMon.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginLockout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEndUtc",
                table: "ManagerAccounts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FailedLoginAttempts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedLoginAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FailedLoginAttempts_ManagerAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "ManagerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FailedLoginAttempts_AccountId_OccurredAtUtc",
                table: "FailedLoginAttempts",
                columns: new[] { "AccountId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FailedLoginAttempts");

            migrationBuilder.DropColumn(
                name: "LockoutEndUtc",
                table: "ManagerAccounts");
        }
    }
}
