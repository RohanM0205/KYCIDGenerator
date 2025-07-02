using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace KYCIDGenerator.Controllers
{
    public class AccountController : Controller
    {
        [HttpPost]
        public IActionResult Logout()
        {

            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // You can clear session/tempdata if you want
            TempData.Clear();
            //HttpContext.Session.Clear();

            // Clear browser cache
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return RedirectToAction("Login", "Auth"); // or redirect to home
        }
    }
}
