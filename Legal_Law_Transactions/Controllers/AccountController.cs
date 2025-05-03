    using Legal_Law_Transactions.Models;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Identity;
    using System.Net.Http;
    using System.Text.Json;
    using Microsoft.Extensions.Configuration;
using Dropbox.Sign.Client;

namespace Legal_Law_Transactions.Controllers
    {
        public class AccountController : Controller
        {
            private readonly ApplicationDbContext _context;
            private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
            private readonly IConfiguration _configuration;

            public AccountController(ApplicationDbContext context, IConfiguration configuration)
            {
            _context = context;
            _configuration = configuration;
        }

          
            [HttpGet]
            public IActionResult Register()
            {
                return View();
            }


            [HttpPost]
            public IActionResult Register(string lastname, string firstname, string email, string password)
            {
                var exists = _context.Users.Any(u => u.email == email);
                if (exists)
                {
                    ViewBag.Error = "Email already exists.";
                    return View();
                }

                var newUser = new User
                {
                    lastname = lastname,
                    firstname = firstname,
                    email = email,
                    role = "Citizen"
                };

              
                newUser.password = _passwordHasher.HashPassword(newUser, password);

                _context.Users.Add(newUser);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }


           
            [HttpGet]
        public IActionResult Login()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (!string.IsNullOrEmpty(role))
            {
                return role switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin"),
                    "Citizen" => RedirectToAction("Dashboard", "Citizen"),
                    "LawEnforcer" => RedirectToAction("Dashboard", "Enforcer"),
                    "Lawyer" => RedirectToAction("Dashboard", "Lawyer"),
                    "Prosecutor" => RedirectToAction("Dashboard", "Prosecutor"),
                    _ => View()
                };
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, [FromForm(Name = "g-recaptcha-response")] string gRecaptchaResponse, string returnUrl = null)
        {
            var secretKey = _configuration["GoogleReCaptcha:SecretKey"];
            var httpClient = new HttpClient();

            var googleReply = await httpClient.GetStringAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={gRecaptchaResponse}"
            );

            var captchaResult = JsonSerializer.Deserialize<ReCaptchaResponse>(googleReply);

            if (captchaResult == null || !captchaResult.success)
            {
                ViewBag.Error = "reCAPTCHA verification failed. Please try again.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.email == email);
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.password, password) != PasswordVerificationResult.Success)
            {
                ViewBag.Error = "Invalid login credentials.";
                return View();
            }

            // 🔐 Sign the user in
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.email),
        new Claim(ClaimTypes.Role, user.role),
        new Claim("FullName", $"{user.firstname} {user.lastname}")
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            HttpContext.Session.SetString("UserEmail", user.email);
            HttpContext.Session.SetString("UserRole", user.role);

            // Test it immediately
            var testEmail = HttpContext.Session.GetString("UserEmail");
            Console.WriteLine("Session Email Set: " + testEmail);


            // ✅ Redirect to original destination
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return user.role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Citizen" => RedirectToAction("Dashboard", "Citizen"),
                "Enforcer" => RedirectToAction("Dashboard", "Enforcer"),
                "Lawyer" => RedirectToAction("Dashboard", "Lawyer"),
                "Prosecutor" => RedirectToAction("Dashboard", "Prosecutor"),
                _ => RedirectToAction("Login")
            };
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var sessionLogId = HttpContext.Session.GetInt32("SessionLogId");

            if (sessionLogId.HasValue)
            {
                var log = _context.SessionLogs.Find(sessionLogId.Value);
                if (log != null)
                {
                    _context.SessionLogs.Remove(log);
                    _context.SaveChanges();
                }
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }



    }
}