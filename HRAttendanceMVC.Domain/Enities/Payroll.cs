using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRAttendanceMVC.Domain.Enities;

public class Payroll
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    [Required]
    [Display(Name = "Month")]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Display(Name = "Year")]
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Required]
    [Display(Name = "Basic Salary")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal BasicSalary { get; set; }

    [Display(Name = "HRA")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal HRA { get; set; }

    [Display(Name = "Other Allowances")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherAllowances { get; set; }

    [Display(Name = "Deductions")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Deductions { get; set; }

    [Display(Name = "Net Salary")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetSalary => BasicSalary + HRA + OtherAllowances - Deductions;

    [StringLength(500)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Status")]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending / Paid

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // HR who created this payroll record
    public int HrUserId { get; set; }

    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
}
