using System.Collections.ObjectModel;
using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public sealed class LevelService : ILevelService
    {
        // WeaponId -> wymagany level
        // Level 1: tylko nożyczki (Id=10)
        // Kolejne levele odblokowują po 2 bronie (ostatni może odblokować 1).
        private static readonly IReadOnlyDictionary<int, int> _weaponRequiredLevels =
            new ReadOnlyDictionary<int, int>(new Dictionary<int, int>
            {
                [10] = 1,   // Scissors (starter)

                [1] = 5,    // Kitchen Knife
                [2] = 10,   // Dagger

                [3] = 20,   // Machete
                [6] = 25,   // Spear

                [4] = 35,   // Sword
                [7] = 45,   // Cleaver

                [5] = 55,   // Axe
                [8] = 65,   // Mace

                [9] = 75    // Katana (after rebalance!)
            });

        private const double LevelGrowthFactor = 1250d;
        private const double LevelGrowthExponent = 2.15d;

        public int GetLevelFromTotalScore(int totalScore)
        {
            var score = Math.Max(0, totalScore);

            var level = 1;
            while (score >= GetNextLevelTotalScoreThreshold(level))
            {
                level++;
            }

            return level;
        }

        public int GetTotalScoreThresholdForLevel(int level)
        {
            if (level < 1)
            {
                return 0;
            }

            if (level == 1)
            {
                return 0;
            }

            var threshold = LevelGrowthFactor * Math.Pow(level - 1, LevelGrowthExponent);
            return (int)Math.Round(threshold, MidpointRounding.AwayFromZero);
        }

        public double GetLevelMultiplier(int level)
        {
            // Kenshi-style diminishing returns: (1 - level/101)^2
            // Trzymamy się zakresu 1..100+, ale zabezpieczamy wartości skrajne.
            var clampedLevel = Math.Max(0, Math.Min(101, level));
            var factor = 1.0 - (clampedLevel / 101.0);
            return factor * factor;
        }

        public int GetNextLevelTotalScoreThreshold(int currentLevel)
        {
            var next = currentLevel + 1;
            if (next < 1)
            {
                next = 1;
            }

            return GetTotalScoreThresholdForLevel(next);
        }

        public void SyncLevel(Level level, int totalScore)
        {
            ArgumentNullException.ThrowIfNull(level);

            var normalizedScore = Math.Max(0, totalScore);
            var currentLevel = GetLevelFromTotalScore(normalizedScore);

            level.CurrentLevel = currentLevel;
            level.CurrentLevelThreshold = GetTotalScoreThresholdForLevel(currentLevel);
            level.NextLevelThreshold = GetNextLevelTotalScoreThreshold(currentLevel);
            level.TotalScoreSnapshot = normalizedScore;
            level.UpdatedAt = DateTime.Now;
        }

        public int GetRequiredLevelForWeapon(int weaponId)
        {
            return _weaponRequiredLevels.TryGetValue(weaponId, out var required) ? required : int.MaxValue;
        }

        public bool IsWeaponUnlocked(int weaponId, int currentLevel)
        {
            var required = GetRequiredLevelForWeapon(weaponId);
            return currentLevel >= required;
        }
    }
}
