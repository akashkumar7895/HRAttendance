using HRAttendanceMVC.Application.DTOs;
using HRAttendanceMVC.Domain.Helpers;
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

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? hrUserId = null)
    {
        var activeEmployees = await _unitOfWork.Employees.GetActiveCountAsync(hrUserId);
        var todayPresent = await _unitOfWork.Attendances.GetPresentCountByDateAsync(TimeHelper.Today, hrUserId);
        var pendingLeaves = await _unitOfWork.LeaveRequests.GetPendingCountAsync(hrUserId);
        var totalLeaveTypes = await _unitOfWork.LeaveTypes.GetCountAsync(hrUserId);

        return new DashboardSummaryDto
        {
            ActiveEmployees = activeEmployees,
            TodayPresent = todayPresent,
            PendingLeaves = pendingLeaves,
            TotalLeaveTypes = totalLeaveTypes
        };
    }

    public async Task<EmployeeDashboardDto> GetEmployeeDashboardSummaryAsync(string email)
    {
        var emp = await _unitOfWork.Employees.GetByEmailAsync(email);
        if (emp == null)
        {
            return new EmployeeDashboardDto { IsFound = false };
        }

        var today = TimeHelper.Today;
        var attendances = await _unitOfWork.Attendances.FindAsync(x => x.EmployeeId == emp.Id);
        var todayAtt = attendances.FirstOrDefault(x => x.AttendanceDate.Date == today);

        var leaveRequests = await _unitOfWork.LeaveRequests.GetByEmployeeIdWithDetailsAsync(emp.Id);

        string? workDuration = null;
        if (todayAtt?.CheckIn.HasValue == true && todayAtt?.CheckOut.HasValue == true)
        {
            var diff = todayAtt.CheckOut.Value - todayAtt.CheckIn.Value;
            var h = (int)Math.Max(0, diff.TotalHours);
            var m = Math.Max(0, diff.Minutes);
            var s = Math.Max(0, diff.Seconds);
            workDuration = $"{h:D2}h {m:D2}m {s:D2}s";
        }
        else if (todayAtt?.CheckIn.HasValue == true)
        {
            var nowTime = TimeHelper.Now.TimeOfDay;
            if (nowTime > todayAtt.CheckIn.Value)
            {
                var diff = nowTime - todayAtt.CheckIn.Value;
                var h = (int)diff.TotalHours;
                var m = diff.Minutes;
                var s = diff.Seconds;
                workDuration = $"{h:D2}h {m:D2}m {s:D2}s";
            }
            else
            {
                workDuration = "00h 00m 00s";
            }
        }

        return new EmployeeDashboardDto
        {
            IsFound = true,
            Employee = emp,
            TodayAttendanceId = todayAtt?.Id,
            TodayAttendanceStatus = todayAtt != null ? todayAtt.Status : "Not Marked",
            TodayCheckIn = TimeHelper.Format12(todayAtt?.CheckIn),
            TodayCheckOut = TimeHelper.Format12(todayAtt?.CheckOut),
            TodayCheckInTime = todayAtt?.CheckIn,
            TodayCheckOutTime = todayAtt?.CheckOut,
            FormattedWorkingDuration = workDuration,
            Remarks = todayAtt?.Remarks,
            TotalPresentDays = attendances.Count(x => x.Status == "Present"),
            PendingLeavesCount = leaveRequests.Count(x => x.Status == "Pending"),
            ApprovedLeavesCount = leaveRequests.Count(x => x.Status == "Approved"),
            RejectedLeavesCount = leaveRequests.Count(x => x.Status == "Rejected"),
            RecentAttendances = attendances.OrderByDescending(x => x.AttendanceDate).Take(5).ToList(),
            RecentLeaves = leaveRequests.Take(5).ToList()
        };
    }

    public async Task<int> GetLeaveTypeCountForHrAsync(int hrUserId)
    {
        return await _unitOfWork.LeaveTypes.GetCountAsync(hrUserId);
    }
}
