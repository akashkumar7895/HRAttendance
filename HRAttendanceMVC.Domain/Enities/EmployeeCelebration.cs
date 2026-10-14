using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Domain.Enities;

public class EmployeeCelebration
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string CelebrationType { get; set; } = "Birthday"; // Birthday, WorkAnniversary

    [StringLength(80)]
    public string? Department { get; set; }

    [StringLength(80)]
    public string? Designation { get; set; }

    [Required]
    public DateTime CelebrationDate { get; set; } = DateTime.Today;

    public int? YearsOfService { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? HrUserId { get; set; }
}
