namespace Symulator_Nozownika.Models
{
    public class UserStatistics
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int TotalScore { get; set; }

       public int TotalClicks { get; set; }
        public int HighestScore { get; set; }
        public TimeSpan TotalPlayTime { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime LastPlayedAt { get; set; }

        public UserAccount User { get; set; } = null!;
    }
}