using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRAttendanceMVC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingOffboardingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OffboardingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    EmployeeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ResignationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastWorkingDay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    IsManagerHandoverDone = table.Column<bool>(type: "bit", nullable: false),
                    IsItCleared = table.Column<bool>(type: "bit", nullable: false),
                    IsFinanceCleared = table.Column<bool>(type: "bit", nullable: false),
                    IsExitInterviewDone = table.Column<bool>(type: "bit", nullable: false),
                    IsFnfCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HrUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OffboardingRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportingManager = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    HasDocumentsSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    HasAssetAssigned = table.Column<bool>(type: "bit", nullable: false),
                    HasEmailCreated = table.Column<bool>(type: "bit", nullable: false),
                    HasOrientationDone = table.Column<bool>(type: "bit", nullable: false),
                    HasBankDetailsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HrUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OffboardingRecords");

            migrationBuilder.DropTable(
                name: "OnboardingRecords");
        }
    }
}
