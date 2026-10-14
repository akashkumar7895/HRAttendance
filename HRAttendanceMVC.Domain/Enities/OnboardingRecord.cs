using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Domain.Enities;

public class OnboardingRecord
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string CandidateName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required, StringLength(80)]
    public string Department { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Designation { get; set; } = string.Empty;

    [Required]
    public DateTime JoiningDate { get; set; } = DateTime.Today;

    [StringLength(100)]
    public string? ReportingManager { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed

    public int ProgressPercentage { get; set; } = 0;

    // Onboarding Checklist Items
    public bool HasDocumentsSubmitted { get; set; } = false;
    public bool HasAssetAssigned { get; set; } = false;
    public bool HasEmailCreated { get; set; } = false;
    public bool HasOrientationDone { get; set; } = false;
    public bool HasBankDetailsVerified { get; set; } = false;

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? HrUserId { get; set; }

    // Calculates progress percentage based on 5 key onboarding steps
    public void RecalculateProgress()
    {
        int count = 0;
        if (HasDocumentsSubmitted) count++;
        if (HasAssetAssigned) count++;
        if (HasEmailCreated) count++;
        if (HasOrientationDone) count++;
        if (HasBankDetailsVerified) count++;

        ProgressPercentage = count * 20;
        if (ProgressPercentage == 100)
        {
            Status = "Completed";
        }
        else if (ProgressPercentage > 0)
        {
            Status = "In Progress";
        }
        else
        {
            Status = "Pending";
        }
    }
}
