using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class PurchasedPotion
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int PotionId { get; set; }

        public int Quantity { get; set; } = 1;

        public int PricePaid { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(UserId))]
        public virtual UserAccount User { get; set; } = null!;

        [ForeignKey(nameof(PotionId))]
        public virtual Potion Potion { get; set; } = null!;
    }
}