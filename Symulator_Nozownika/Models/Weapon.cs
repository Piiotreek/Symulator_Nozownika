
using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class Weapon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int Damage { get; set; }

        public double Cooldown { get; set; }

        public string ImageUrl { get; set; }
    }
}
