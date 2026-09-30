namespace HRAttendanceMVC.Infrastructure.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IEmployeeRepository Employees { get; }
    IAttendanceRepository Attendances { get; }
    ILeaveRequestRepository LeaveRequests { get; }
    ILeaveTypeRepository LeaveTypes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
