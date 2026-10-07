using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Interfaces;
using HRAttendanceMVC.Application.Interfaces;

namespace HRAttendanceMVC.Application.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending", "Approved", "Rejected"
    };

    private readonly IUnitOfWork _unitOfWork;

    public LeaveRequestService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LeaveRequest>> GetAllLeaveRequestsAsync(int? hrUserId = null)
    {
        return await _unitOfWork.LeaveRequests.GetAllWithDetailsAsync(hrUserId);
    }

    public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id)
    {
        return await _unitOfWork.LeaveRequests.GetByIdWithDetailsAsync(id);
    }

    public async Task<ServiceResult<LeaveRequest>> CreateLeaveRequestAsync(LeaveRequest request)
    {
        if (request.ToDate < request.FromDate)
        {
            return ServiceResult<LeaveRequest>.Failed(
                "To Date must be greater than or equal to From Date.", 
                nameof(request.ToDate));
        }

        var employeeExists = await _unitOfWork.Employees.ExistsAsync(x => x.Id == request.EmployeeId);
        if (!employeeExists)
        {
            return ServiceResult<LeaveRequest>.Failed("Selected employee does not exist.", nameof(request.EmployeeId));
        }

        var leaveTypeExists = await _unitOfWork.LeaveTypes.ExistsAsync(x => x.Id == request.LeaveTypeId);
        if (!leaveTypeExists)
        {
            return ServiceResult<LeaveRequest>.Failed("Selected leave type does not exist.", nameof(request.LeaveTypeId));
        }

        request.CreatedAt = DateTime.Now;
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            request.Status = "Pending";
        }

        await _unitOfWork.LeaveRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<LeaveRequest>.Ok(request, "Leave request created successfully.");
    }

    public async Task<ServiceResult> UpdateStatusAsync(int id, string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !ValidStatuses.Contains(status))
        {
            return ServiceResult.Failed("Invalid leave status.");
        }

        var item = await _unitOfWork.LeaveRequests.GetByIdAsync(id);
        if (item == null)
        {
            return ServiceResult.Failed("Leave request not found.");
        }

        item.Status = status;
        _unitOfWork.LeaveRequests.Update(item);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok($"Leave request {status.ToLowerInvariant()}.");
    }

    public async Task<int> GetPendingLeaveCountAsync(int? hrUserId = null)
    {
        return await _unitOfWork.LeaveRequests.GetPendingCountAsync(hrUserId);
    }
}
