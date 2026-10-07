using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Infrastructure.Interfaces;

public interface ILeaveTypeRepository : IRepository<LeaveType>
{
    Task<IReadOnlyList<LeaveType>> GetAllOrderedByNameAsync(int? hrUserId = null);
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null, int? hrUserId = null);
    Task<bool> HasLeaveRequestsAsync(int leaveTypeId);
    Task<int> GetCountAsync(int? hrUserId = null);
}
