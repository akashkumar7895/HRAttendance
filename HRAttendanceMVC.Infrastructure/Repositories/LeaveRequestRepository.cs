using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Data;
using HRAttendanceMVC.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRAttendanceMVC.Infrastructure.Repositories;

public class LeaveRequestRepository : Repository<LeaveRequest>, ILeaveRequestRepository
{
    public LeaveRequestRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LeaveRequest>> GetAllWithDetailsAsync()
    {
        return await DbSet
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
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

    public async Task<int> GetPendingCountAsync()
    {
        return await DbSet.CountAsync(x => x.Status == "Pending");
    }

    public async Task<bool> HasRequestsForLeaveTypeAsync(int leaveTypeId)
    {
        return await DbSet.AnyAsync(x => x.LeaveTypeId == leaveTypeId);
    }

    public async Task<bool> HasRequestsForEmployeeAsync(int employeeId)
    {
        return await DbSet.AnyAsync(x => x.EmployeeId == employeeId);
    }
}
