
using HRAttendanceMVC.Application.DTOs;

namespace HRAttendanceMVC.Application.Interfaces;
public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? hrUserId = null);
    Task<EmployeeDashboardDto> GetEmployeeDashboardSummaryAsync(string email);
    Task<int> GetLeaveTypeCountForHrAsync(int hrUserId);
}
