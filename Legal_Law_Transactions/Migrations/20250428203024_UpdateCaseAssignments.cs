using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legal_Law_Transactions.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCaseAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "assigned_to",
                table: "Schedules",
                newName: "assigned_to_lawyer");

            migrationBuilder.RenameColumn(
                name: "assigned_by",
                table: "Schedules",
                newName: "assigned_to_citizen");

            migrationBuilder.RenameColumn(
                name: "assigned_to",
                table: "Evidences",
                newName: "assigned_to_lawyer");

            migrationBuilder.RenameColumn(
                name: "assigned_by",
                table: "Evidences",
                newName: "assigned_to_citizen");

            migrationBuilder.RenameColumn(
                name: "assigned_to",
                table: "Documents",
                newName: "assigned_to_lawyer");

            migrationBuilder.RenameColumn(
                name: "assigned_by",
                table: "Documents",
                newName: "assigned_to_citizen");

            migrationBuilder.RenameColumn(
                name: "assigned_to",
                table: "Cases",
                newName: "assigned_to_lawyer");

            migrationBuilder.RenameColumn(
                name: "assigned_by",
                table: "Cases",
                newName: "assigned_to_citizen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "assigned_to_lawyer",
                table: "Schedules",
                newName: "assigned_to");

            migrationBuilder.RenameColumn(
                name: "assigned_to_citizen",
                table: "Schedules",
                newName: "assigned_by");

            migrationBuilder.RenameColumn(
                name: "assigned_to_lawyer",
                table: "Evidences",
                newName: "assigned_to");

            migrationBuilder.RenameColumn(
                name: "assigned_to_citizen",
                table: "Evidences",
                newName: "assigned_by");

            migrationBuilder.RenameColumn(
                name: "assigned_to_lawyer",
                table: "Documents",
                newName: "assigned_to");

            migrationBuilder.RenameColumn(
                name: "assigned_to_citizen",
                table: "Documents",
                newName: "assigned_by");

            migrationBuilder.RenameColumn(
                name: "assigned_to_lawyer",
                table: "Cases",
                newName: "assigned_to");

            migrationBuilder.RenameColumn(
                name: "assigned_to_citizen",
                table: "Cases",
                newName: "assigned_by");
        }
    }
}
