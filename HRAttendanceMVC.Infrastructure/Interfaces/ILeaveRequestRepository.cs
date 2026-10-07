using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Infrastructure.Interfaces;

public interface ILeaveRequestRepository : IRepository<LeaveRequest>
{
    Task<IReadOnlyList<LeaveRequest>> GetAllWithDetailsAsync(int? hrUserId = null);
    Task<LeaveRequest?> GetByIdWithDetailsAsync(int id);
    Task<int> GetPendingCountAsync(int? hrUserId = null);
    Task<bool> HasRequestsForLeaveTypeAsync(int leaveTypeId);
    Task<bool> HasRequestsForEmployeeAsync(int employeeId);
    Task<IReadOnlyList<LeaveRequest>> GetByEmployeeIdWithDetailsAsync(int employeeId);
}
