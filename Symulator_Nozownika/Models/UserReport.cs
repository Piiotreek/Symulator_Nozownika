using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class UserReport
    {
        public int Id { get; set; }

        [Required]
        public int ReportedUserId { get; set; }

        [ForeignKey("ReportedUserId")]
        public virtual UserAccount ReportedUser { get; set; }

        [Required]
        public int ReportedByUserId { get; set; }

        [ForeignKey("ReportedByUserId")]
        public virtual UserAccount ReportedByUser { get; set; }

        [Required]
        public ReportReason Reason { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public int? ReviewedByAdminId { get; set; }

        [ForeignKey("ReviewedByAdminId")]
        public virtual UserAccount? ReviewedByAdmin { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string AdminNotes { get; set; }
    }

    public enum ReportReason
    {
        OffensiveNickname = 0,
        InappropriateAvatar = 1,
        PornographicContent = 2,
        Impersonation = 3,
        Harassment = 4,
        Spam = 5,
        Other = 6
    }
}
