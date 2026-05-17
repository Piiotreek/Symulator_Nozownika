using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Type your current password.")]
        [DataType(DataType.Password)]
        [Display(Name = "current password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type your new password.")]
        [DataType(DataType.Password)]
        [Display(Name = "new password")]
        [StringLength(100, ErrorMessage = " password must be at least 6 characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
