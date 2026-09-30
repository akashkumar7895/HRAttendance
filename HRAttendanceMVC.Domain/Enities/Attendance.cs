using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Domain.Enities;

public class Attendance
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Attendance Date")]
    public DateTime AttendanceDate { get; set; } = DateTime.Today;

    [Required, StringLength(20)]
    public string Status { get; set; } = "Present";

    [DataType(DataType.Time)]
    public TimeSpan? CheckIn { get; set; }

    [DataType(DataType.Time)]
    public TimeSpan? CheckOut { get; set; }

    [StringLength(300)]
    public string? Remarks { get; set; }
}
