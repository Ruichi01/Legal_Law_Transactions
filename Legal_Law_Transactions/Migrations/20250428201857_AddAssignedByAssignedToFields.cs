using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legal_Law_Transactions.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedByAssignedToFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "assigned_by",
                table: "Schedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "assigned_to",
                table: "Schedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "assigned_by",
                table: "Evidences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "assigned_to",
                table: "Evidences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "assigned_by",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "assigned_to",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "assigned_by",
                table: "Cases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "assigned_to",
                table: "Cases",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "assigned_by",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "assigned_to",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "assigned_by",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "assigned_to",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "assigned_by",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "assigned_to",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "assigned_by",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "assigned_to",
                table: "Cases");
        }
    }
}
