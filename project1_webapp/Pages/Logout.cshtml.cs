using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace project1_webapp.Pages
{
    public class LogoutModel : PageModel
    {
        public void OnGet()
        {
        }
        public IActionResult OnGetOtherTask() {
            HttpContext.Session.Clear();
            return Page();
        }
    }
}
