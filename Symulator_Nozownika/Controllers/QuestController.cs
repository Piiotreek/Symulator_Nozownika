using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Symulator_Nozownika.Services;
using System.Security.Claims;

namespace Symulator_Nozownika.Controllers
{
    [Authorize]
    public class QuestController : Controller
    {
        private readonly IQuestService _questService;

        public QuestController(IQuestService questService)
        {
            _questService = questService;
        }

        // GET: /Quest/Index
        public async Task<IActionResult> Index()
        {
            // Block demo users from quests
            if (User.FindFirst("IsDemo")?.Value == "true")
            {
                TempData["DemoError"] = "Quests are not available in Demo mode";
                return RedirectToAction("SecurePage", "Account");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var quests = await _questService.GetTodayQuestsAsync(userId);
            return View(quests);
        }

        // POST: /Quest/ClaimReward
        [HttpPost]
        public async Task<IActionResult> ClaimReward([FromBody] ClaimRewardRequest request)
        {
            // Block demo users from claiming quest rewards
            if (User.FindFirst("IsDemo")?.Value == "true")
            {
                return Json(new { success = false, message = "Quest rewards are not available in Demo mode" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _questService.ClaimRewardAsync(userId, request.QuestProgressId);

            if (success)
                return Json(new { success = true, message = "Nagroda odebrana!" });

            return Json(new { success = false, message = "Nie możesz odebrać tej nagrody." });
        }

        public class ClaimRewardRequest
        {
            public int QuestProgressId { get; set; }
        }
    }
}
