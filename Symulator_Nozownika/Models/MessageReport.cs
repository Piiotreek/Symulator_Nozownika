using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class MessageReport
    {
        public int Id { get; set; }

        [Required]
        public int ReportedMessageId { get; set; }

        [ForeignKey("ReportedMessageId")]
        public virtual ClubMessage ReportedMessage { get; set; }

        [Required]
        public int ReportedByUserId { get; set; }

        [ForeignKey("ReportedByUserId")]
        public virtual UserAccount ReportedByUser { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public int? ReviewedByAdminId { get; set; }

        [ForeignKey("ReviewedByAdminId")]
        public virtual UserAccount? ReviewedByAdmin { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string AdminNotes { get; set; } = string.Empty;
    }

    public enum ReportStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Resolved = 3
    }
}
