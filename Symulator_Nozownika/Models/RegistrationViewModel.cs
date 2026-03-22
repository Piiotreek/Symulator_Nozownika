using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "First Name is Required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is Required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is Required")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please Enter Valid Email.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Username is Required")]
        [MaxLength(20,ErrorMessage ="Max 20 characters are allowed")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        [StringLength(20, ErrorMessage = "The Password must be at least 6 characters long.", MinimumLength = 6)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm Password is Required")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}
