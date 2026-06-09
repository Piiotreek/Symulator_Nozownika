using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class HighScore
    {
        public int Id { get; set; }

        [ForeignKey("UserAccount")]
        public int? UserId { get; set; }

        public virtual UserAccount? UserAccount { get; set; }

        [Required]
        public int Score { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public string? PlayerName { get; set; } // Dla anonimowych graczy
        public string? Country { get; set; }
        public string? CountryFlag { get; set; }
    }
}
