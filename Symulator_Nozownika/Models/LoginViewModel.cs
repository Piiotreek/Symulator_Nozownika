using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username or Email is Required")]
        [MaxLength(30, ErrorMessage = "Max 30 characters are allowed")]
        [Display(Name = "Username or Email")]
        public string UserNameOrEmail { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        [StringLength(20, ErrorMessage = "The Password must be at least 6 characters long.", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
