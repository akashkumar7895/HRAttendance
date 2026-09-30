using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Data;
using HRAttendanceMVC.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRAttendanceMVC.Infrastructure.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Employee>> GetAllOrderedByNameAsync()
    {
        return await DbSet.OrderBy(x => x.FirstName).ToListAsync();
    }

    public async Task<IReadOnlyList<Employee>> GetActiveEmployeesOrderedByNameAsync()
    {
        return await DbSet
            .Where(x => x.IsActive)
            .OrderBy(x => x.FirstName)
            .ToListAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string employeeCode, int? excludeId = null)
    {
        return !await DbSet.AnyAsync(x => 
            x.EmployeeCode == employeeCode && (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    public async Task<bool> HasRelatedRecordsAsync(int employeeId)
    {
        var hasAttendance = await Context.Attendances.AnyAsync(x => x.EmployeeId == employeeId);
        if (hasAttendance) return true;

        var hasLeaves = await Context.LeaveRequests.AnyAsync(x => x.EmployeeId == employeeId);
        return hasLeaves;
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await DbSet.CountAsync(x => x.IsActive);
    }
}
