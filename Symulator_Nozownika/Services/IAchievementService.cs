namespace Symulator_Nozownika.Services
{
    public interface IAchievementService
    {
        Task CheckTotalScoreAchievementsAsync(int userId, int newTotalScore);
        //for future checking other types of achievements, like total games played, total clicks, etc.
    }
}
