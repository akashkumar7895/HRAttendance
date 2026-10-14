using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Domain.Enities;

public class EngagementPost
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string AuthorName { get; set; } = "Team Member";

    [Required, StringLength(1500)]
    public string Content { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Category { get; set; } = "Shoutout"; // Shoutout, Activity, Culture, Tech

    public int LikesCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? HrUserId { get; set; }
}
