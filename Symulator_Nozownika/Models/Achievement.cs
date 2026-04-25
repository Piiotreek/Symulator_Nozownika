using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class Achievement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        public AchievementType Type { get; set; }

        //for example: 2000 clicks, 100 games played, etc.
        public int TargetValue { get; set; }
    }
}