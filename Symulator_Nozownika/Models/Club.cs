using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Symulator_Nozownika.Models
{
    public class Club
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Club name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Club name must be between 3 and 100 characters")]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required]
        public int OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual UserAccount Owner { get; set; } = null!;

        [Required] public string OwnerName { get; set; }
        public DateTime CreatedAt { get; set; }

        public int MaxMembers { get; set; } = 20;

        // Relacja do członków klubu
        public virtual ICollection<ClubMember> Members { get; set; } = new List<ClubMember>();
        public virtual ICollection<ClubMessage> Messages { get; set; } = new List<ClubMessage>();
        
        public int GetAvailableSpots()
        {
            return MaxMembers - Members.Count;
        }

        public bool IsFull()
        {
            return Members.Count >= MaxMembers;
        }

        public bool IsOwner(int userId)
        {
            return OwnerId == userId;
        }
    }
}
