using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Data;
using HRAttendanceMVC.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRAttendanceMVC.Infrastructure.Repositories;

public class LeaveTypeRepository : Repository<LeaveType>, ILeaveTypeRepository
{
    public LeaveTypeRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LeaveType>> GetAllOrderedByNameAsync(int? hrUserId = null)
    {
        var query = DbSet.AsQueryable();
        if (hrUserId.HasValue) query = query.Where(x => x.HrUserId == hrUserId.Value);
        return await query.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null, int? hrUserId = null)
    {
        var query = DbSet.Where(x => x.Name == name);
        if (excludeId.HasValue) query = query.Where(x => x.Id != excludeId.Value);
        if (hrUserId.HasValue) query = query.Where(x => x.HrUserId == hrUserId.Value);
        return !await query.AnyAsync();
    }

    public async Task<bool> HasLeaveRequestsAsync(int leaveTypeId)
    {
        return await Context.LeaveRequests.AnyAsync(x => x.LeaveTypeId == leaveTypeId);
    }

    public async Task<int> GetCountAsync(int? hrUserId = null)
    {
        var query = DbSet.AsQueryable();
        if (hrUserId.HasValue) query = query.Where(x => x.HrUserId == hrUserId.Value);
        return await query.CountAsync();
    }
}
