using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRAttendanceMVC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsReadToContactMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "ContactMessages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "ContactMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
