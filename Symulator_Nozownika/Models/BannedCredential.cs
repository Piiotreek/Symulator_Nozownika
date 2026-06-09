using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class BannedCredential
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        public DateTime BannedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}