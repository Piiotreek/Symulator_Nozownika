using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class FavoriteWeapon
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int WeaponId { get; set; }

        public DateTime MarkedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual UserAccount User { get; set; }

        [ForeignKey("WeaponId")]
        public virtual Weapon Weapon { get; set; }
    }
}