namespace Symulator_Nozownika.Models
{
    public class PublicProfileViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }
        public DateTime MemberSince { get; set; }

        // Broń
        public string WeaponName { get; set; } = "Pięści";
        public string? WeaponImageUrl { get; set; }
        public int WeaponDamage { get; set; } = 10;

        // Poziom
        public int CurrentLevel { get; set; } = 1;

        // Statystyki
        public int TotalGamesPlayed { get; set; }
        public int TotalScore { get; set; }
        public int HighestScore { get; set; }
        public int TotalClicks { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        

        // Klan
        public string? ClubName { get; set; }
        public int? ClubId { get; set; }

        // Osiągnięcia
        public List<AchievementInfo> Achievements { get; set; } = new();

        public IEnumerable<SavedScore> SavedScores { get; set; } = new List<SavedScore>();
    }

    public class AchievementInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public DateTime UnlockedAt { get; set; }
    }

    public class TopScoreInfo
    {
        public int Score { get; set; }
        public DateTime AchievedAt { get; set; }
    }
}
