namespace Symulator_Nozownika.Services
{
    public interface ILevelService
    {
        int GetLevelFromTotalScore(int totalScore);
        int GetTotalScoreThresholdForLevel(int level);
        int? GetNextLevelTotalScoreThreshold(int currentLevel);

        double GetLevelMultiplier(int level);

        int GetRequiredLevelForWeapon(int weaponId);
        bool IsWeaponUnlocked(int weaponId, int currentLevel);
    }
}
