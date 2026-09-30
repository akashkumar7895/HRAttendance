using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Infrastructure.Interfaces;

public interface ILeaveTypeRepository : IRepository<LeaveType>
{
    Task<IReadOnlyList<LeaveType>> GetAllOrderedByNameAsync();
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    Task<bool> HasLeaveRequestsAsync(int leaveTypeId);
}
