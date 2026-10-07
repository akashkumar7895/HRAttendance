using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Interfaces;

public interface ILeaveRequestService
{
    Task<IReadOnlyList<LeaveRequest>> GetAllLeaveRequestsAsync(int? hrUserId = null);
    Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id);
    Task<ServiceResult<LeaveRequest>> CreateLeaveRequestAsync(LeaveRequest request);
    Task<ServiceResult> UpdateStatusAsync(int id, string status);
    Task<int> GetPendingLeaveCountAsync(int? hrUserId = null);
}
