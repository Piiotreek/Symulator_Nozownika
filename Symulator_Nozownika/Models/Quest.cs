using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class Quest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public QuestType QuestType { get; set; }

        [Required]
        public int TargetValue { get; set; }

        [Required]
        public int RewardCoins { get; set; }

        [Required]
        public int RewardXp { get; set; }

        [Required]
        public DateTime ResetsAt { get; set; }
    }
}
