using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class DemoLockout
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime LockedUntil { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsLocked => DateTime.UtcNow < LockedUntil;
    }
}
