using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Nazwa użytkownika lub adres e-mail są wymagane")]
        [MaxLength(30, ErrorMessage = "Maksymalnie dozwolone jest 30 znaków")]
        [Display(Name = "Nazwa użytkownika lub e-mail")]
        public string UserNameOrEmail { get; set; }
        [Required(ErrorMessage = "Hasło jest wymagane")]
        [DataType(DataType.Password)]
        [StringLength(20, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.", MinimumLength = 6)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
