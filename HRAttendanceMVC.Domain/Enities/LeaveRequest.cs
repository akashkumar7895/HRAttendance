using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Domain.Enities;

public class LeaveRequest
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required]
    public int LeaveTypeId { get; set; }
    public LeaveType? LeaveType { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateTime FromDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateTime ToDate { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int TotalDays => ToDate < FromDate ? 0 : (ToDate.Date - FromDate.Date).Days + 1;
}
