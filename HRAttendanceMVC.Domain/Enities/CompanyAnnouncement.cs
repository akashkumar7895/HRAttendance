using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Domain.Enities;

public class CompanyAnnouncement
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Category { get; set; } = "General"; // Policy, Milestone, Culture, Tech, General

    [Required, StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    [StringLength(100)]
    public string AuthorName { get; set; } = "HR Team";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? HrUserId { get; set; }
}
