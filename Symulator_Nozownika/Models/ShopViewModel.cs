using System.Collections.Generic;
using System.Linq;

namespace Symulator_Nozownika.Models
{
    public class ShopViewModel
    {
        public int UserLevel { get; set; }

        public int UserTotalScore { get; set; }

        public int CurrentLevelThreshold { get; set; }

        public int? NextLevelThreshold { get; set; }

        public int CoinBalance { get; set; }

        public int? SelectedWeaponId { get; set; }

        public List<int> PurchasedWeaponIds { get; set; } = new();

        public List<int> PurchasedUpgradeIds { get; set; } = new();

        public Dictionary<int, int> OwnedPotionQuantities { get; set; } = new();

        public List<int> FavoriteWeaponIds { get; set; } = new();

        public IReadOnlyList<Weapon> Weapons { get; set; } = new List<Weapon>();

        public IReadOnlyList<Potion> Potions { get; set; } = new List<Potion>();

        public IReadOnlyList<WeaponUpgrade> WeaponUpgrades { get; set; } = new List<WeaponUpgrade>();

        public IEnumerable<Weapon> FavoriteWeapons => Weapons.Where(w => FavoriteWeaponIds.Contains(w.Id));

        public IEnumerable<Weapon> OwnedWeapons => Weapons.Where(w => PurchasedWeaponIds.Contains(w.Id) && !FavoriteWeaponIds.Contains(w.Id));

        public IEnumerable<Weapon> NonOwnedWeapons => Weapons.Where(w => !PurchasedWeaponIds.Contains(w.Id) && !FavoriteWeaponIds.Contains(w.Id));
    }
}
