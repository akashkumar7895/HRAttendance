using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.DTOs;

public class EmployeeDashboardDto
{
    public bool IsFound { get; set; }
    public Employee? Employee { get; set; }
    public string TodayAttendanceStatus { get; set; } = "Not Marked";
    public string? TodayCheckIn { get; set; }
    public string? TodayCheckOut { get; set; }
    public int TotalPresentDays { get; set; }
    public int PendingLeavesCount { get; set; }
    public int ApprovedLeavesCount { get; set; }
    public int RejectedLeavesCount { get; set; }
    public IReadOnlyList<Attendance> RecentAttendances { get; set; } = new List<Attendance>();
    public IReadOnlyList<LeaveRequest> RecentLeaves { get; set; } = new List<LeaveRequest>();
}
