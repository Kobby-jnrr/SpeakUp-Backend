using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpeakUp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailVerificationToken",
                table: "Users",
                newName: "EmailVerificationCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailVerificationCode",
                table: "Users",
                newName: "EmailVerificationToken");
        }
    }
}
