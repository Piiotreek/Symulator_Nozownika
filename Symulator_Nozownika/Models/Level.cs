using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class Level
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public int CurrentLevel { get; set; } = 1;

        public int CurrentLevelThreshold { get; set; }

        public int NextLevelThreshold { get; set; }

        public int TotalScoreSnapshot { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual UserAccount User { get; set; } = null!;
    }
}