using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public interface IQuestService
    {
        /// <summary>Zwraca questy z postępem użytkownika na dziś (tworzy wpisy jeśli brak).</summary>
        Task<List<UserQuestProgress>> GetTodayQuestsAsync(int userId);

        /// <summary>Aktualizuje postęp kliknięć i score po grze; zwraca nowo ukończone questy.</summary>
        Task<List<Quest>> UpdateProgressAsync(int userId, int gameScore, int gameClicks);

        /// <summary>Odbiera nagrodę za ukończony quest (monety + XP). Zwraca false jeśli nieukończony lub już odebrano.</summary>
        Task<bool> ClaimRewardAsync(int userId, int questProgressId);
    }
}
