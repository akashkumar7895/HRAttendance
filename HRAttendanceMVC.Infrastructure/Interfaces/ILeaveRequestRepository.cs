using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Infrastructure.Interfaces;

public interface ILeaveRequestRepository : IRepository<LeaveRequest>
{
    Task<IReadOnlyList<LeaveRequest>> GetAllWithDetailsAsync();
    Task<LeaveRequest?> GetByIdWithDetailsAsync(int id);
    Task<int> GetPendingCountAsync();
    Task<bool> HasRequestsForLeaveTypeAsync(int leaveTypeId);
    Task<bool> HasRequestsForEmployeeAsync(int employeeId);
}
