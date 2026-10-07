using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Data;
using HRAttendanceMVC.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRAttendanceMVC.Infrastructure.Repositories;

public class LeaveRequestRepository : Repository<LeaveRequest>, ILeaveRequestRepository
{
    public LeaveRequestRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LeaveRequest>> GetAllWithDetailsAsync(int? hrUserId = null)
    {
        var query = DbSet
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .AsQueryable();

        if (hrUserId.HasValue)
        {
            query = query.Where(x => x.Employee != null && x.Employee.HrUserId == hrUserId.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<LeaveRequest?> GetByIdWithDetailsAsync(int id)
    {
        return await DbSet
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<int> GetPendingCountAsync(int? hrUserId = null)
    {
        var query = DbSet.Where(x => x.Status == "Pending");

        if (hrUserId.HasValue)
        {
            query = query.Where(x => x.Employee != null && x.Employee.HrUserId == hrUserId.Value);
        }

        return await query.CountAsync();
    }

    public async Task<bool> HasRequestsForLeaveTypeAsync(int leaveTypeId)
    {
        return await DbSet.AnyAsync(x => x.LeaveTypeId == leaveTypeId);
    }

    public async Task<bool> HasRequestsForEmployeeAsync(int employeeId)
    {
        return await DbSet.AnyAsync(x => x.EmployeeId == employeeId);
    }

    public async Task<IReadOnlyList<LeaveRequest>> GetByEmployeeIdWithDetailsAsync(int employeeId)
    {
        return await DbSet
            .Include(x => x.LeaveType)
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
