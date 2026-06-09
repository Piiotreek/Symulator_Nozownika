using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class WeaponUpgrade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        public int WeaponId { get; set; }

        public int Price { get; set; }

        public int DamageBonus { get; set; }

        public double CooldownReduction { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        [ForeignKey(nameof(WeaponId))]
        public virtual Weapon Weapon { get; set; } = null!;
    }
}