using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class PurchasedWeapon
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int WeaponId { get; set; }

        public int PricePaid { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual UserAccount User { get; set; } = null!;

        [ForeignKey("WeaponId")]
        public virtual Weapon Weapon { get; set; } = null!;
    }
}