using Microsoft.EntityFrameworkCore.Migrations;

namespace Legal_Law_Transactions.Migrations
{
    public partial class AddFilePathToDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adding the file_path column to the Documents table
            migrationBuilder.AddColumn<string>(
                name: "file_path",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Removing the file_path column from the Documents table
            migrationBuilder.DropColumn(
                name: "file_path",
                table: "Documents");
        }
    }
}
