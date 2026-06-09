using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class SavedScore
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual UserAccount User { get; set; } = null!;

        [Required]
        public int Score { get; set; }

        public DateTime AchievedAt { get; set; } = DateTime.Now;

        public DateTime SavedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? Note { get; set; }
    }
}
