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
        public async Task<IActionResult> Register(string lastname, string firstname, string email, string password, IFormFile applicationDocument, string feedback)
        {
            var exists = _context.Users.Any(u => u.email == email);
            if (exists)
            {
                ViewBag.Error = "Email already exists.";
                return View();
            }

            if (applicationDocument == null || applicationDocument.ContentType != "application/pdf")
            {
                ViewBag.Error = "Please upload a valid PDF file.";
                return View();
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "applications");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(applicationDocument.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await applicationDocument.CopyToAsync(fileStream);
            }

            var newUser = new User
            {
                lastname = lastname,
                firstname = firstname,
                email = email,
                role = "Applicant",
                status = "Pending",
                
            };

            newUser.password = _passwordHasher.HashPassword(newUser, password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(); 

            var application = new Application
            {
                user_id = newUser.user_id,              
                file_path = filePath,                   
                submitted_at = DateTime.Now,             
                status = "Pending",
                feedback = string.Empty
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync(); 

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, newUser.email),
        new Claim(ClaimTypes.Role, newUser.role),
        new Claim("FullName", $"{newUser.firstname} {newUser.lastname}")
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            HttpContext.Session.SetString("UserEmail", newUser.email);
            HttpContext.Session.SetString("UserRole", newUser.role);
            HttpContext.Session.SetString("UserId", newUser.user_id.ToString());
            HttpContext.Session.SetString("FirstName", newUser.firstname);
            HttpContext.Session.SetString("LastName", newUser.lastname);

            return RedirectToAction("Pending");
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
            HttpContext.Session.SetString("UserId", user.user_id.ToString());
            HttpContext.Session.SetString("FirstName", user.firstname);
            HttpContext.Session.SetString("LastName", user.lastname);

            var testEmail = HttpContext.Session.GetString("UserEmail");
            Console.WriteLine("Session Email Set: " + testEmail);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.role == "Applicant")
            {
                return RedirectToAction("Pending");
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
        public IActionResult Pending()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userRole) || userRole != "Applicant")
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadApplicationForm(IFormFile applicationFile)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            var application = _context.Applications.FirstOrDefault(a => a.user_id == currentUser.user_id);

            if (applicationFile == null || applicationFile.ContentType != "application/pdf")
            {
                TempData["Error"] = "Please upload a valid PDF file.";
                return RedirectToAction("Pending");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "applications");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(applicationFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            if (!string.IsNullOrEmpty(application?.file_path) && System.IO.File.Exists(application.file_path))
            {
                System.IO.File.Delete(application.file_path);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await applicationFile.CopyToAsync(fileStream);
            }

            application.file_path = filePath; 
            application.status = "Pending";  

            _context.Applications.Update(application);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your application form has been re-uploaded successfully and is now pending approval.";

            return RedirectToAction("Pending");
        }


    }
}