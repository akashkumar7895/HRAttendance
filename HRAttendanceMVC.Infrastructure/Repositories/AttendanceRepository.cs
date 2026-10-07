using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Data;
using HRAttendanceMVC.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRAttendanceMVC.Infrastructure.Repositories;

public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Attendance>> GetByDateWithEmployeeAsync(DateTime date, int? hrUserId = null)
    {
        var targetDate = date.Date;
        var query = DbSet
            .Include(x => x.Employee)
            .Where(x => x.AttendanceDate.Date == targetDate);

        if (hrUserId.HasValue)
        {
            query = query.Where(x => x.Employee != null && x.Employee.HrUserId == hrUserId.Value);
        }

        return await query
            .OrderBy(x => x.Employee != null ? x.Employee.FirstName : string.Empty)
            .ToListAsync();
    }

    public async Task<Attendance?> GetByIdWithEmployeeAsync(int id)
    {
        return await DbSet
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsForEmployeeAndDateAsync(int employeeId, DateTime date, int? excludeId = null)
    {
        var targetDate = date.Date;
        return await DbSet.AnyAsync(x =>
            x.EmployeeId == employeeId &&
            x.AttendanceDate.Date == targetDate &&
            (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    public async Task<int> GetPresentCountByDateAsync(DateTime date, int? hrUserId = null)
    {
        var targetDate = date.Date;
        var query = DbSet
            .Where(x => x.AttendanceDate.Date == targetDate && x.Status == "Present");

        if (hrUserId.HasValue)
        {
            query = query.Where(x => x.Employee != null && x.Employee.HrUserId == hrUserId.Value);
        }

        return await query.CountAsync();
    }
}
