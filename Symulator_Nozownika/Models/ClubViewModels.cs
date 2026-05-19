using System.ComponentModel.DataAnnotations;

namespace Symulator_Nozownika.Models
{
    public class CreateClubViewModel
    {
        [Required(ErrorMessage = "Club name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Club name must be between 3 and 100 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class ClubDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public int AvailableSpots { get; set; }
        public int MaxMembers { get; set; }
        public List<ClubMemberViewModel> Members { get; set; } = new();
        public bool IsOwner { get; set; }
        public bool IsMember { get; set; }
        public bool IsFull { get; set; }
    }

    public class ClubMemberViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Country { get; set; }
        public ClubRole Role { get; set; }
    }

    public class ClubListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string OwnerName { get; set; }
        public int MemberCount { get; set; }
        public int AvailableSpots { get; set; }
        public int MaxMembers { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFull { get; set; }
        public bool IsMember { get; set; }
        public bool IsOwner { get; set; }
    }
}
