using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Symulator_Nozownika.Views.Admin
{
    [Authorize(Roles = "Admin")]
    public class ReportsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
