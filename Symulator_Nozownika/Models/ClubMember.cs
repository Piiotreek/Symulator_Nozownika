using System;

namespace Symulator_Nozownika.Models
{
    public class ClubMember
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public int UserId { get; set; }
        public ClubRole Role { get; set; }
        public DateTime JoinedAt { get; set; }

        public virtual Club Club { get; set; }
        public virtual UserAccount User { get; set; }
    }

    public enum ClubRole
    {
        Member = 0,
        Moderator = 1,
        Owner = 2
    }
}