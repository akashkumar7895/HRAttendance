# HR Attendance Management - ASP.NET Core MVC .NET 8

Modules included:
- Dashboard
- Employee CRUD
- Attendance management
- Leave Type CRUD
- Leave Request create/approve/reject
- EF Core SQL Server database
- Automatic migration at startup
- Seed data for Leave Types and sample Employees

## Run
1. Open `HRAttendanceMVC.sln` in Visual Studio 2022.
2. Make sure SQL Server LocalDB is installed.
3. Check `appsettings.json` connection string.
4. Restore NuGet packages.
5. Run the project with F5.

The application calls `Database.Migrate()` on startup, so the database/tables are created automatically when migrations exist.

If you prefer Package Manager Console:
```powershell
Add-Migration InitialCreate
Update-Database
```

Then run:
- `/Dashboard`
- `/Employee`
- `/Attendance`
- `/LeaveType`
- `/LeaveRequest`

Important: `DbInitializer` only inserts seed data. It does not create the `LeaveTypes` table. EF Core migration/database creation happens before seeding.
