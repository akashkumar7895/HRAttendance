using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Domain.Enities;

public class LeaveType
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = "";

    [Range(0, 365)]
    [Display(Name = "Total Days")]
    public int TotalDays { get; set; }

    public int HrUserId { get; set; }
}
