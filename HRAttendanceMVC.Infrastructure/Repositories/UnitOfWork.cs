using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Data;
using HRAttendanceMVC.Infrastructure.Interfaces;

namespace HRAttendanceMVC.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IEmployeeRepository? _employees;
    private IAttendanceRepository? _attendances;
    private ILeaveRequestRepository? _leaveRequests;
    private ILeaveTypeRepository? _leaveTypes;
    private bool _disposed;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IEmployeeRepository Employees =>
        _employees ??= new EmployeeRepository(_context);

    public IAttendanceRepository Attendances =>
        _attendances ??= new AttendanceRepository(_context);

    public ILeaveRequestRepository LeaveRequests =>
        _leaveRequests ??= new LeaveRequestRepository(_context);

    public ILeaveTypeRepository LeaveTypes =>
        _leaveTypes ??= new LeaveTypeRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
