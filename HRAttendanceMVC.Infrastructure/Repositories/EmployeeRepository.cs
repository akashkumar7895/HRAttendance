using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Data;
using HRAttendanceMVC.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRAttendanceMVC.Infrastructure.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Employee>> GetAllOrderedByNameAsync(int? hrUserId = null)
    {
        var query = DbSet.AsQueryable();
        if (hrUserId.HasValue) query = query.Where(x => x.HrUserId == hrUserId.Value);
        return await query.OrderBy(x => x.FirstName).ToListAsync();
    }

    public async Task<IReadOnlyList<Employee>> GetActiveEmployeesOrderedByNameAsync(int? hrUserId = null)
    {
        var query = DbSet.Where(x => x.IsActive);
        if (hrUserId.HasValue) query = query.Where(x => x.HrUserId == hrUserId.Value);
        return await query.OrderBy(x => x.FirstName).ToListAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string employeeCode, int? excludeId = null, int? hrUserId = null)
    {
        var query = DbSet.Where(x => x.EmployeeCode == employeeCode);
        if (excludeId.HasValue) query = query.Where(x => x.Id != excludeId.Value);
        if (hrUserId.HasValue) query = query.Where(x => x.HrUserId == hrUserId.Value);
        return !await query.AnyAsync();
    }

    public async Task<bool> HasRelatedRecordsAsync(int employeeId)
    {
        var hasAttendance = await Context.Attendances.AnyAsync(x => x.EmployeeId == employeeId);
        if (hasAttendance) return true;

        var hasLeaves = await Context.LeaveRequests.AnyAsync(x => x.EmployeeId == employeeId);
        return hasLeaves;
    }

    public async Task<int> GetActiveCountAsync(int? hrUserId = null)
    {
        var query = DbSet.Where(x => x.IsActive);
        if (hrUserId.HasValue) query = query.Where(x => x.HrUserId == hrUserId.Value);
        return await query.CountAsync();
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
    }
}
