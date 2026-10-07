using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRAttendanceMVC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHrUserIdToEmployeeAndLeaveType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaveTypes_Name",
                table: "LeaveTypes");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "HrUserId",
                table: "LeaveTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HrUserId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Existing rows ko pehle wale User ki ID assign karo (agar Users table empty nahi hai)
            // Ye ensure karta hai FK constraint pass ho
            migrationBuilder.Sql(@"
                DECLARE @FirstUserId INT = (SELECT TOP 1 Id FROM Users ORDER BY Id);
                IF @FirstUserId IS NOT NULL
                BEGIN
                    UPDATE Employees SET HrUserId = @FirstUserId WHERE HrUserId = 0;
                    UPDATE LeaveTypes SET HrUserId = @FirstUserId WHERE HrUserId = 0;
                END
            ");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_HrUserId",
                table: "LeaveTypes",
                column: "HrUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_Name",
                table: "LeaveTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_HrUserId",
                table: "Employees",
                column: "HrUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Users_HrUserId",
                table: "Employees",
                column: "HrUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveTypes_Users_HrUserId",
                table: "LeaveTypes",
                column: "HrUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Users_HrUserId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveTypes_Users_HrUserId",
                table: "LeaveTypes");

            migrationBuilder.DropIndex(
                name: "IX_LeaveTypes_HrUserId",
                table: "LeaveTypes");

            migrationBuilder.DropIndex(
                name: "IX_LeaveTypes_Name",
                table: "LeaveTypes");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_HrUserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HrUserId",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "HrUserId",
                table: "Employees");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_Name",
                table: "LeaveTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);
        }
    }
}
