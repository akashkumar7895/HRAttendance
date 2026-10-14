using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Domain.Enities;

public class CompanyEvent
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string EventType { get; set; } = "CompanyEvent"; // Holiday, CompanyEvent

    [StringLength(50)]
    public string? SubType { get; set; } // Gazetted, Optional, Town Hall, Sports, Hackathon, Wellness

    [Required]
    public DateTime EventDate { get; set; } = DateTime.Today;

    [StringLength(150)]
    public string? Location { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public int RsvpCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? HrUserId { get; set; }
}
