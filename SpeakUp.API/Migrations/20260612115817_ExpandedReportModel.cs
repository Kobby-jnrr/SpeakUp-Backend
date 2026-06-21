using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpeakUp.API.Migrations
{
    /// <inheritdoc />
    public partial class ExpandedReportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComplainantGender",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplainantStudentId",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplaintNature",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Confidential",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesiredOutcome",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentDate",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentLocation",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentTime",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriorReportWhere",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelationshipToComplainant",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RespondentDepartment",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RespondentName",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RespondentPosition",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Witness1Contact",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Witness1Name",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Witness2Contact",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Witness2Name",
                table: "Reports",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplainantGender",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ComplainantStudentId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ComplaintNature",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Confidential",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DesiredOutcome",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IncidentDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IncidentLocation",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IncidentTime",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PriorReportWhere",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RelationshipToComplainant",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RespondentDepartment",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RespondentName",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RespondentPosition",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Witness1Contact",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Witness1Name",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Witness2Contact",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Witness2Name",
                table: "Reports");
        }
    }
}
