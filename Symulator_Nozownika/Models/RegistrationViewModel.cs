using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class RegistrationViewModel
    {
        [Display(Name = "Imię")]
        [Required(ErrorMessage = "Imię jest wymagane")]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        public string LastName { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Adres e-mail jest wymagany")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|((([\w-]+\.)+)))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Podaj poprawny adres e-mail")]
        public string Email { get; set; }

        [Display(Name = "Kraj")]
        [Required(ErrorMessage = "Kraj jest wymagany")]
        public string Country { get; set; }

        [Display(Name = "Nazwa użytkownika")]
        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
        [MaxLength(20, ErrorMessage = "Maksymalnie dozwolone jest 20 znaków")]
        public string UserName { get; set; }

        [Display(Name = "Hasło")]
        [Required(ErrorMessage = "Hasło jest wymagane")]
        [DataType(DataType.Password)]
        [StringLength(20, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków", MinimumLength = 6)]
        public string Password { get; set; }

        [Display(Name = "Potwierdź hasło")]
        [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hasło i potwierdzenie hasła muszą być takie same")]
        public string ConfirmPassword { get; set; }

    }
}
