using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SampleApp.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}