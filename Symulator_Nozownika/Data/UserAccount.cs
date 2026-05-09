using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class UserAccount
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is Required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is Required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is Required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Country is Required")]
        public string Country { get; set; }
        [Required(ErrorMessage = "Username is Required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Klucz obcy
        public int? SelectedWeaponId { get; set; }

        // WŁAŚCIWOŚĆ NAWIGACYJNA - Broń
        [ForeignKey("SelectedWeaponId")]
        public virtual Weapon? SelectedWeapon { get; set; }

        // WŁAŚCIWOŚĆ NAWIGACYJNA - Statystyki (DODAJ TO!)
        public virtual UserStatistics? Statistics { get; set; }

        public virtual Level? Level { get; set; }

        public virtual CoinWallet? CoinWallet { get; set; }

        public virtual ICollection<PurchasedWeapon> PurchasedWeapons { get; set; } = new List<PurchasedWeapon>();

        // Ścieżka do awatara
        public string? AvatarPath { get; set; }

        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}
