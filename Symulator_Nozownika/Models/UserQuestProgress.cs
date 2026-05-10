using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class UserQuestProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserAccount User { get; set; } = null!;

        [Required]
        public int QuestId { get; set; }

        [ForeignKey(nameof(QuestId))]
        public virtual Quest Quest { get; set; } = null!;

        public int CurrentValue { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsRewardClaimed { get; set; }

        /// <summary>Data (tylko dzień), dla której jest aktywny ten postęp. Reset po zmianie dnia.</summary>
        public DateTime ProgressDate { get; set; } = DateTime.UtcNow.Date;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
