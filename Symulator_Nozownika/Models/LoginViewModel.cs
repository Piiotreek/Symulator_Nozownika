using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is Required")]
        [MaxLength(20, ErrorMessage = "Max 20 characters are allowed")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        [StringLength(20, ErrorMessage = "The Password must be at least 6 characters long.", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
