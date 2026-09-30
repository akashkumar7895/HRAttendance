using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Interfaces;

public interface ILeaveTypeService
{
    Task<IReadOnlyList<LeaveType>> GetAllLeaveTypesAsync();
    Task<LeaveType?> GetLeaveTypeByIdAsync(int id);
    Task<ServiceResult<LeaveType>> CreateLeaveTypeAsync(LeaveType leaveType);
    Task<ServiceResult<LeaveType>> UpdateLeaveTypeAsync(int id, LeaveType leaveType);
    Task<ServiceResult> DeleteLeaveTypeAsync(int id);
    Task<int> GetTotalLeaveTypesCountAsync();
}
