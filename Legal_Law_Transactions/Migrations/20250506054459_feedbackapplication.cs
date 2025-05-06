using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legal_Law_Transactions.Migrations
{
    /// <inheritdoc />
    public partial class feedbackapplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "feedback",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "feedback",
                table: "Applications");
        }
    }
}
