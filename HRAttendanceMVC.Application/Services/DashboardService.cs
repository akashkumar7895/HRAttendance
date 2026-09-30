using HRAttendanceMVC.Application.DTOs;
using HRAttendanceMVC.Infrastructure.Interfaces;
using HRAttendanceMVC.Application.Interfaces;

namespace HRAttendanceMVC.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var activeEmployees = await _unitOfWork.Employees.GetActiveCountAsync();
        var todayPresent = await _unitOfWork.Attendances.GetPresentCountByDateAsync(DateTime.Today);
        var pendingLeaves = await _unitOfWork.LeaveRequests.GetPendingCountAsync();
        var totalLeaveTypes = await _unitOfWork.LeaveTypes.CountAsync();

        return new DashboardSummaryDto
        {
            ActiveEmployees = activeEmployees,
            TodayPresent = todayPresent,
            PendingLeaves = pendingLeaves,
            TotalLeaveTypes = totalLeaveTypes
        };
    }
}
