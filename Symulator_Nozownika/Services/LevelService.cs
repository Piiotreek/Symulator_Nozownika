using System.Collections.ObjectModel;

namespace Symulator_Nozownika.Services
{
    public sealed class LevelService : ILevelService
    {
        // Level = 1..N; progi oznaczają minimalny TotalScore wymagany do wejścia na dany level.
        // Index 0 ignorujemy (dla czytelności), a dla level 1 próg = 0.
        private static readonly IReadOnlyList<int> _levelScoreThresholds = new ReadOnlyCollection<int>(
        [
            0,      // (unused)
            0,      // level 1
            2000,   // level 2
            5000,   // level 3
            10000,  // level 4
            20000,  // level 5
            35000   // level 6
        ]);

        // WeaponId -> wymagany level
        // Level 1: tylko nożyczki (Id=10)
        // Kolejne levele odblokowują po 2 bronie (ostatni może odblokować 1).
        private static readonly IReadOnlyDictionary<int, int> _weaponRequiredLevels =
            new ReadOnlyDictionary<int, int>(new Dictionary<int, int>
            {
                [10] = 1, // Scissors

                [1] = 2,  // Kitchen Knife
                [2] = 2,  // Dagger

                [3] = 3,  // Machete
                [6] = 3,  // Spear

                [4] = 4,  // Sword
                [9] = 4,  // Katana

                [7] = 5,  // Cleaver
                [5] = 5,  // Axe

                [8] = 6   // Mace
            });

        public int GetLevelFromTotalScore(int totalScore)
        {
            var score = Math.Max(0, totalScore);

            // Iterujemy od najwyższego progu w dół, by znaleźć najwyższy osiągnięty level.
            for (var level = _levelScoreThresholds.Count - 1; level >= 1; level--)
            {
                if (score >= _levelScoreThresholds[level])
                {
                    return level;
                }
            }

            return 1;
        }

        public int GetTotalScoreThresholdForLevel(int level)
        {
            if (level < 1)
            {
                return 0;
            }

            if (level >= _levelScoreThresholds.Count)
            {
                return _levelScoreThresholds[^1];
            }

            return _levelScoreThresholds[level];
        }

        public double GetLevelMultiplier(int level)
        {
            // Kenshi-style diminishing returns: (1 - level/101)^2
            // Trzymamy się zakresu 1..100+, ale zabezpieczamy wartości skrajne.
            var clampedLevel = Math.Max(0, Math.Min(101, level));
            var factor = 1.0 - (clampedLevel / 101.0);
            return factor * factor;
        }

        public int? GetNextLevelTotalScoreThreshold(int currentLevel)
        {
            var next = currentLevel + 1;
            if (next < 1 || next >= _levelScoreThresholds.Count)
            {
                return null;
            }

            return _levelScoreThresholds[next];
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
