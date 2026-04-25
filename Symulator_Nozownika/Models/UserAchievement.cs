using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class UserAchievement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserAccountId { get; set; }
        [ForeignKey("UserAccountId")]
        public virtual UserAccount User { get; set; }

        [Required]
        public int AchievementId { get; set; }
        [ForeignKey("AchievementId")]
        public virtual Achievement Achievement { get; set; }

        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}
