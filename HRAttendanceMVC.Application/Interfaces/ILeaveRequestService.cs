using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Interfaces;

public interface ILeaveRequestService
{
    Task<IReadOnlyList<LeaveRequest>> GetAllLeaveRequestsAsync();
    Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id);
    Task<ServiceResult<LeaveRequest>> CreateLeaveRequestAsync(LeaveRequest request);
    Task<ServiceResult> UpdateStatusAsync(int id, string status);
    Task<int> GetPendingLeaveCountAsync();
}
