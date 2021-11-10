using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace SampleApp.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        [Route("login")]
        public async Task Login(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync("signicat", new AuthenticationProperties()
            {
                RedirectUri = returnUrl
            });
        }
        
        [Route("logout")]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("signicat", new AuthenticationProperties()
            {
                RedirectUri = "/",
            });

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}