using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legal_Law_Transactions.Migrations
{
    /// <inheritdoc />
    public partial class YourMigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Users_user_id",
                table: "Cases");

            migrationBuilder.DropForeignKey(
                name: "FK_Licenses_Users_user_id",
                table: "Licenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_Cases_case_id",
                table: "Records");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_Users_user_id1",
                table: "Records");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Cases_case_id",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_Documents_document_id",
                table: "Signatures");

            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_Users_user_id",
                table: "Signatures");

            migrationBuilder.DropIndex(
                name: "IX_Records_user_id1",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "user_id1",
                table: "Records");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_user_id",
                table: "Documents",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Users_user_id",
                table: "Cases",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_user_id",
                table: "Documents",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Licenses_Users_user_id",
                table: "Licenses",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Cases_case_id",
                table: "Records",
                column: "case_id",
                principalTable: "Cases",
                principalColumn: "case_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Cases_case_id",
                table: "Schedules",
                column: "case_id",
                principalTable: "Cases",
                principalColumn: "case_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_Documents_document_id",
                table: "Signatures",
                column: "document_id",
                principalTable: "Documents",
                principalColumn: "document_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_Users_user_id",
                table: "Signatures",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Users_user_id",
                table: "Cases");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_user_id",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Licenses_Users_user_id",
                table: "Licenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_Cases_case_id",
                table: "Records");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Cases_case_id",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_Documents_document_id",
                table: "Signatures");

            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_Users_user_id",
                table: "Signatures");

            migrationBuilder.DropIndex(
                name: "IX_Documents_user_id",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "Documents");

            migrationBuilder.AddColumn<int>(
                name: "user_id1",
                table: "Records",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Records_user_id1",
                table: "Records",
                column: "user_id1");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Users_user_id",
                table: "Cases",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Licenses_Users_user_id",
                table: "Licenses",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Cases_case_id",
                table: "Records",
                column: "case_id",
                principalTable: "Cases",
                principalColumn: "case_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Users_user_id1",
                table: "Records",
                column: "user_id1",
                principalTable: "Users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Cases_case_id",
                table: "Schedules",
                column: "case_id",
                principalTable: "Cases",
                principalColumn: "case_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_Documents_document_id",
                table: "Signatures",
                column: "document_id",
                principalTable: "Documents",
                principalColumn: "document_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_Users_user_id",
                table: "Signatures",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
