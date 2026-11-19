using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DubaiJobs.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployerIdToPostJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployerId",
                table: "PostJobs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployerId",
                table: "PostJobs");
        }
    }
}
