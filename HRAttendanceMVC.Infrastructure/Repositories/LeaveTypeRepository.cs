using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Data;
using HRAttendanceMVC.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRAttendanceMVC.Infrastructure.Repositories;

public class LeaveTypeRepository : Repository<LeaveType>, ILeaveTypeRepository
{
    public LeaveTypeRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LeaveType>> GetAllOrderedByNameAsync()
    {
        return await DbSet.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
    {
        return !await DbSet.AnyAsync(x => 
            x.Name == name && (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    public async Task<bool> HasLeaveRequestsAsync(int leaveTypeId)
    {
        return await Context.LeaveRequests.AnyAsync(x => x.LeaveTypeId == leaveTypeId);
    }
}
