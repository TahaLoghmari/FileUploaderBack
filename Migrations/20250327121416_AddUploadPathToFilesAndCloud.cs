using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileUploaderBack.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadPathToFilesAndCloud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UploadPath",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadPath",
                table: "Files");
        }
    }
}
