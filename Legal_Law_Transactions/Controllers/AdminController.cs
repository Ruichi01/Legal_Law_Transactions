using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Legal_Law_Transactions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Legal_Law_Transactions.Services;

namespace Legal_Law_Transactions.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAdminLogService _adminLogService;

        public AdminController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, IAdminLogService adminLogService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _adminLogService = adminLogService;
        }
        [HttpPost]
        public async Task<IActionResult> AddUser(string email, string firstName, string lastName, string role, string password)
        {
            var user = new User
            {
                email = email,
                firstname = firstName,
                lastname = lastName,
                role = role
            };

            user.password = _passwordHasher.HashPassword(user, password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var adminIdString = HttpContext.Session.GetString("UserId"); 
            if (int.TryParse(adminIdString, out int adminId))
            {
                _adminLogService.LogAction(adminId, "Add User", "User", $"Added user {firstName} {lastName} (Role: {role})");
            }

            return RedirectToAction("Users");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUser(int user_id, string email, string firstName, string lastName, string role, string password)
        {
            var user = await _context.Users.FindAsync(user_id);
            if (user == null) return NotFound();

            var changes = new List<string>();
            var adminIdString = HttpContext.Session.GetString("UserId");

            if (user.firstname != firstName)
            {
                changes.Add($"First Name changed from '{user.firstname}' to '{firstName}'");
                user.firstname = firstName;
            }

            if (user.lastname != lastName)
            {
                changes.Add($"Last Name changed from '{user.lastname}' to '{lastName}'");
                user.lastname = lastName;
            }

            if (user.role != role)
            {
                changes.Add($"Role changed from '{user.role}' to '{role}'");
                user.role = role;
            }

            user.email = email;

            if (!string.IsNullOrEmpty(password))
            {
                user.password = _passwordHasher.HashPassword(user, password);
                changes.Add($"Password for User ID #{user_id} (Role: {user.role}) has been updated");
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            if (int.TryParse(adminIdString, out int adminId) && changes.Any())
            {
                foreach (var change in changes)
                {
                    _adminLogService.LogAction(adminId, "Update User", "User", change);
                }
            }

            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCaseStatus(int case_id, string status)
        {
            var caseItem = await _context.Cases.FindAsync(case_id);
            if (caseItem == null)
            {
                return NotFound();
            }

            caseItem.status = status;
            await _context.SaveChangesAsync();

            var adminIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(adminIdString, out int adminId))
            {
                _adminLogService.LogAction(adminId, "Update Case", "Case", $"Updated status of case #{case_id} to '{status}'");
            }

            return RedirectToAction("Cases");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateDocumentsStatus(int document_id, string notarized)
        {
            var document = await _context.Documents.FindAsync(document_id);
            if (document == null)
            {
                return NotFound();
            }

            document.notarized = notarized;
            await _context.SaveChangesAsync();

            var adminIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(adminIdString, out int adminId))
            {
                _adminLogService.LogAction(adminId, "Update Document", "Document", $"Updated status of document #{document_id} to '{notarized}'");
            }

            return RedirectToAction("Document");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateLicenseStatus(int license_id, string type, string status)
        {
            var license = await _context.Licenses.FindAsync(license_id);
            if (license == null)
            {
                return NotFound();
            }

            license.type = type;
            license.status = status;

            await _context.SaveChangesAsync();

            var adminIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(adminIdString, out int adminId))
            {
                _adminLogService.LogAction(adminId, "Update License", "License", $"Updated license #{license_id} with new type '{type}' and status '{status}'");
            }

            return RedirectToAction("License");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateRecordStatus(int record_id, string status)
        {
            var record = await _context.Records.FindAsync(record_id);
            if (record == null)
            {
                return NotFound();
            }

            record.status = status;
            await _context.SaveChangesAsync();

            var adminIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(adminIdString, out int adminId))
            {
                _adminLogService.LogAction(adminId, "Update Record", "Record", $"Updated status of record #{record_id} to '{status}'");
            }

            return RedirectToAction("Records");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateScheduleStatus(int schedule_id, string status)
        {
            var schedule = await _context.Schedules.FindAsync(schedule_id);
            if (schedule == null)
            {
                return NotFound();
            }

            schedule.status = status;
            await _context.SaveChangesAsync();

            var adminIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(adminIdString, out int adminId))
            {
                _adminLogService.LogAction(adminId, "Update Schedule", "Schedule", $"Updated status of schedule #{schedule_id} to '{status}'");
            }

            return RedirectToAction("Schedules");
        }


        public IActionResult Users()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        public IActionResult Records()
        {
            var records = _context.Records.Include(r => r.User).ToList();
            return View(records);
        }

        public IActionResult License()
        {
            var licenses = _context.Licenses.Include(l => l.User).ToList();
            return View(licenses);
        }

        public IActionResult Signature()
        {
            var signatures = _context.Signatures.Include(s => s.User).Include(s => s.Document).ToList();
            return View(signatures);
        }

        public IActionResult Document()
        {
            var documents = _context.Documents.Include(d => d.User).ToList();
            return View(documents);
        }


        public async Task<IActionResult> Cases()
        {
            var cases = await _context.Cases.Include(c => c.User).ToListAsync();
            return View(cases);
        }

        public IActionResult Schedules()
        {
            var schedules = _context.Schedules.Include(s => s.Case).ToList();
            return View(schedules);
        }
        public IActionResult Logs()
        {
            var logs = _context.AdminLogs
                .Include(l => l.Admin)
                .OrderByDescending(l => l.Timestamp)
                .ToList();

            return View(logs);
        }


        public IActionResult Dashboard()
        {
            var totalUsers = _context.Users.Count();
            var totalRecords = _context.Records.Count();
            var totalCases = _context.Cases.Count();
            var totalLicenses = _context.Licenses.Count();
            var totalSignatures = _context.Signatures.Count();
            var totalDocuments = _context.Documents.Count();
            var totalSchedules = _context.Schedules.Count();

            ViewData["TotalUsers"] = totalUsers;
            ViewData["TotalRecords"] = totalRecords;
            ViewData["TotalCases"] = totalCases;
            ViewData["TotalLicenses"] = totalLicenses;
            ViewData["TotalSignatures"] = totalSignatures;
            ViewData["TotalDocuments"] = totalDocuments;
            ViewData["TotalSchedules"] = totalSchedules;

            return View();
        }

        public IActionResult Profile()
        {
            var firstName = HttpContext.Session.GetString("UserFirstName");
            var lastName = HttpContext.Session.GetString("UserLastName");
            var email = HttpContext.Session.GetString("UserEmail");

            ViewBag.FullName = $"{firstName} {lastName}";
            ViewBag.Email = email;

            return View();
        }
    }
}
