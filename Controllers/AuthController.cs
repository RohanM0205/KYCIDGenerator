using Microsoft.AspNetCore.Mvc;
using KYCIDGenerator.Models;

namespace KYCIDGenerator.Controllers
{
    public class AuthController : Controller
    {
        // Step 1: Show login form
        public IActionResult Login()
        {
            return View();
        }

        // Step 2: Get OTP
        [HttpPost]
        public IActionResult SendOtp(UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            // Clear any existing timer data when starting new session
            // HttpContext.Session.Remove($"otpTimer_{model.Mobile}");

            string generatedOtp = GenerateRandomOtp();
            TempData["Mobile"] = model.Mobile;
            TempData["Otp"] = generatedOtp;
            TempData["FullName"] = model.FullName;
            TempData["ResendCount"] = 0;

            return RedirectToAction("VerifyOtp");
        }

        // Step 3: Show OTP verification page
        public IActionResult VerifyOtp()
        {
            if (TempData["Mobile"] == null || TempData["Otp"] == null)
            {
                return RedirectToAction("Login");
            }

            var model = new OtpVerificationViewModel
            {
                Mobile = TempData["Mobile"]?.ToString()
            };

            // Keep only essential data
            TempData.Keep("Mobile");
            TempData.Keep("Otp");
            TempData.Keep("FullName");
            TempData.Keep("ResendCount");

            // Check for success message from resend
            if (TempData["ResendSuccess"] != null)
            {
                ViewBag.ResendSuccess = TempData["ResendSuccess"];
                TempData.Remove("ResendSuccess");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult ResendOtp()
        {
            if (TempData["Mobile"] == null)
            {
                return Unauthorized();
            }

            var resendCount = TempData["ResendCount"] as int? ?? 0;
            if (resendCount >= 3)
            {
                return StatusCode(429);
            }

            TempData["Otp"] = GenerateRandomOtp();
            TempData["ResendCount"] = resendCount + 1;

            // Remove any existing success message before setting new one
            TempData.Remove("ResendSuccess");
            TempData["ResendSuccess"] = $"New OTP has been sent to {TempData["Mobile"]}";

            TempData.Keep("Mobile");
            TempData.Keep("FullName");
            TempData.Keep("ResendCount");

            return Ok();
        }

        [HttpPost]
        public IActionResult VerifyOtp(OtpVerificationViewModel model)
        {
            if (TempData["Otp"] == null || TempData["Mobile"] == null)
            {
                return RedirectToAction("Login");
            }

            // Clear the resend success message when verifying OTP
            TempData.Remove("ResendSuccess");

            var correctOtp = TempData["Otp"]?.ToString();
            if (model.Otp == correctOtp)
            {
                TempData.Remove("Otp");
                TempData.Remove("ResendCount");
                return RedirectToAction("Home", "Kyc");
            }

            ModelState.AddModelError("Otp", "Invalid OTP entered. Please try again.");

            TempData.Keep("Otp");
            TempData.Keep("FullName");
            TempData.Keep("Mobile");
            TempData.Keep("ResendCount");

            return View(model);
        }

        private string GenerateRandomOtp()
        {
            // Generate a random 6-digit OTP
            //var random = new Random();
            //return random.Next(100000, 999999).ToString();
            return "1234";
        }
    }
}