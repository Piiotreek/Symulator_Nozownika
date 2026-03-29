using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Username is Required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public int? SelectedWeaponId { get; set; }

    }
}
