namespace HRAttendanceMVC.Application.DTOs;

public class DashboardSummaryDto
{
    public int ActiveEmployees { get; set; }
    public int TodayPresent { get; set; }
    public int PendingLeaves { get; set; }
    public int TotalLeaveTypes { get; set; }
}
