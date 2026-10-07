using System.Security.Claims;
using HRAttendanceMVC.Application.DTOs;
using HRAttendanceMVC.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace HRAttendanceMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // =========================================================
        // LOGIN - GET
        // =========================================================
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }


        // =========================================================
        // LOGIN - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            var user = await _authService.LoginAsync(loginDto);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Incorrect email or password!"
                );

                return View(loginDto);
            }

            // =====================================================
            // USER CLAIMS
            // =====================================================

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.Name ?? string.Empty
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email ?? string.Empty
                ),

                
                new Claim(
                    ClaimTypes.Role,
                    string.IsNullOrWhiteSpace(user.Role)
                        ? "Employee"
                        : user.Role
                )
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true
                }
            );

            TempData["Success"] =
                $"Welcome back, {user.Name}";

            return RedirectToAction(
                "Index",
                "Dashboard"
            );
        }


        // =========================================================
        // SIGNUP - GET
        // =========================================================
        [HttpGet]
        public IActionResult Signup()
        {
            if (User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }


        // =========================================================
        // SIGNUP - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupDto signupDto)
        {
            if (!ModelState.IsValid)
            {
                return View(signupDto);
            }

            // =====================================================
            // ALLOWED ROLES
            // =====================================================

            if (signupDto.Role != "HR" &&
                signupDto.Role != "Employee")
            {
                ModelState.AddModelError(
                    "Role",
                    "Please select a valid account role."
                );

                return View(signupDto);
            }

            // =====================================================
            // CREATE ACCOUNT
            // =====================================================

            var result = await _authService.SignupAsync(signupDto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message
                );

                return View(signupDto);
            }

            TempData["Success"] =
                $"{signupDto.Role} account has been created successfully. Please login.";

            return RedirectToAction(
                "Login",
                "Account"
            );
        }


        // =========================================================
        // LOGOUT - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            TempData.Clear();
            TempData["Success"] =
                "You have been successfully logged out.";

            return RedirectToAction(
                "Index",
                "Dashboard"
            );
        }


        // =========================================================
        // LOGOUT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            TempData.Clear();
            TempData["Success"] =
                "You have been successfully logged out.";

            return RedirectToAction(
                "Index",
                "Dashboard"
            );
        }


        // =========================================================
        // ACCESS DENIED
        // =========================================================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}