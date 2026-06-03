using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class Potion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        public int Price { get; set; }

        public int EffectStrength { get; set; }

        public int DurationInSeconds { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }
    }
}