namespace Symulator_Nozownika.Services
{
    using Symulator_Nozownika.Models;

    public interface ILevelService
    {
        int GetLevelFromTotalScore(int totalScore);
        int GetTotalScoreThresholdForLevel(int level);
        int GetNextLevelTotalScoreThreshold(int currentLevel);

        double GetLevelMultiplier(int level);

        void SyncLevel(Level level, int totalScore);

        int GetRequiredLevelForWeapon(int weaponId);
        bool IsWeaponUnlocked(int weaponId, int currentLevel);
        int GetWeaponPrice(int weaponId, int weaponDamage);
        int GetCoinReward(int rawScore, int weaponDamage);
    }
}
