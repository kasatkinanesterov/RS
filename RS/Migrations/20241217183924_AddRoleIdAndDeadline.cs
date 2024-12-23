using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleIdAndDeadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "Services",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "Products",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "OrderServices",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "OrderProducts",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "OrderProducts");
        }
    }
}
