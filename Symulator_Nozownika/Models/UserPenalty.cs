using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class UserPenalty
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserAccount User { get; set; }

        [Required]
        public int AdminId { get; set; }

        [ForeignKey("AdminId")]
        public virtual UserAccount Admin { get; set; }

        [Required]
        public PenaltyType Type { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public int? RelatedReportId { get; set; }

        [ForeignKey("RelatedReportId")]
        public virtual UserReport? RelatedReport { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }

    public enum PenaltyType
    {
        Suspension1Day = 0,      // Zawieszenie 1 dzień
        Suspension7Days = 1,     // Zawieszenie 7 dni
        Ban = 2,                 // Ban
        BanWithDeletion = 3      // Ban z usunięciem konta
    }
}
