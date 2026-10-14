using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Domain.Enities;

public class OffboardingRecord
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    [Required, StringLength(100)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Department { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Designation { get; set; } = string.Empty;

    [Required]
    public DateTime ResignationDate { get; set; } = DateTime.Today;

    [Required]
    public DateTime LastWorkingDay { get; set; } = DateTime.Today.AddDays(30);

    [Required, StringLength(50)]
    public string ExitType { get; set; } = "Resignation"; // Resignation, Retirement, Contract End, Termination

    [StringLength(500)]
    public string? Reason { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "Initiated"; // Initiated, Clearance In Progress, Completed

    public int ProgressPercentage { get; set; } = 0;

    // Clearance Milestones
    public bool IsManagerHandoverDone { get; set; } = false;
    public bool IsItCleared { get; set; } = false;
    public bool IsFinanceCleared { get; set; } = false;
    public bool IsExitInterviewDone { get; set; } = false;
    public bool IsFnfCompleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? HrUserId { get; set; }

    // Calculates progress percentage based on 5 clearance milestones
    public void RecalculateProgress()
    {
        int count = 0;
        if (IsManagerHandoverDone) count++;
        if (IsItCleared) count++;
        if (IsFinanceCleared) count++;
        if (IsExitInterviewDone) count++;
        if (IsFnfCompleted) count++;

        ProgressPercentage = count * 20;
        if (ProgressPercentage == 100)
        {
            Status = "Completed";
        }
        else if (ProgressPercentage > 0)
        {
            Status = "Clearance In Progress";
        }
        else
        {
            Status = "Initiated";
        }
    }
}
