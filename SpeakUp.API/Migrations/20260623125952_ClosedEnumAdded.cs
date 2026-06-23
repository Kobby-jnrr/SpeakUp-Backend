using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpeakUp.API.Migrations
{
    /// <inheritdoc />
    public partial class ClosedEnumAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "Reports");
        }
    }
}
