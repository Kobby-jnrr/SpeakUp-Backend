using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpeakUp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Resources",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Resources");
        }
    }
}
