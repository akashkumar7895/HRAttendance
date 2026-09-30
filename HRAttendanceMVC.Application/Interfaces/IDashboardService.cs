
using HRAttendanceMVC.Application.DTOs;

namespace HRAttendanceMVC.Application.Interfaces;
public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
}
