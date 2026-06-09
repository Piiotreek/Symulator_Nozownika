using System.Collections.ObjectModel;
using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public sealed class LevelService : ILevelService
    {
        public const int StarterWeaponId = 10;

        // Ręczne ceny early-game, żeby start nie był grindem.
        // Balans można później dopracować bez zmiany całej formuły.
        private static readonly IReadOnlyDictionary<int, int> _earlyWeaponPrices =
            new ReadOnlyDictionary<int, int>(new Dictionary<int, int>
            {
                [1] = 200,   // Kitchen Knife
                [2] = 450,   // Dagger
                [3] = 1200   // Machete
            });

        // WeaponId -> wymagany level
        // Level 1: tylko nożyczki (Id=10)
        // Level 5 : Kitchen Knife (Id=1 itd.)
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

        private const int LevelLinearGrowth = 200;
        private const int LevelQuadraticGrowth = 40;
        private const int WeaponBasePrice = 75;
        private const int WeaponLevelLinearPriceGrowth = 60;
        private const int WeaponLevelQuadraticPriceGrowth = 12;

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

            var previousLevels = level - 1;
            var threshold = (LevelQuadraticGrowth * previousLevels * previousLevels) + (LevelLinearGrowth * previousLevels);
            return threshold;
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

        public int GetWeaponPrice(int weaponId, int weaponDamage)
        {
            if (weaponId == StarterWeaponId)
            {
                return 0;
            }

            if (_earlyWeaponPrices.TryGetValue(weaponId, out var earlyPrice))
            {
                return earlyPrice;
            }

            var requiredLevel = GetRequiredLevelForWeapon(weaponId);
            if (requiredLevel == int.MaxValue)
            {
                return int.MaxValue;
            }

            var normalizedDamage = Math.Max(1, weaponDamage);
            var levelComponent = (WeaponLevelQuadraticPriceGrowth * requiredLevel * requiredLevel) +
                                 (WeaponLevelLinearPriceGrowth * requiredLevel);

            return WeaponBasePrice + levelComponent + (normalizedDamage * 15);
        }

        public int GetCoinReward(int rawScore, int weaponDamage)
        {
            var normalizedScore = Math.Max(0, rawScore);
            var normalizedDamage = Math.Max(1, weaponDamage);

            // Wczesna gra: nożyczki mają niski DMG, więc dodajemy stałą bazę,
            // a część score skalujemy łagodniej, żeby pierwsze zakupy były realne.
            const int baseReward = 15;
            var scoreReward = (int)Math.Round(normalizedScore / 30.0, MidpointRounding.AwayFromZero);
            var damageReward = normalizedDamage * 2;

            return Math.Max(5, baseReward + scoreReward + damageReward);
        }
    }
}
