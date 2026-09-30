using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Domain.Enities;

public class Employee
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Employee Code")]
    public string EmployeeCode { get; set; } = "";

    [Required, StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = "";

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required, StringLength(80)]
    public string Department { get; set; } = "";

    [Required, StringLength(80)]
    public string Designation { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Joining Date")]
    public DateTime JoiningDate { get; set; } = DateTime.Today;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}";
}
