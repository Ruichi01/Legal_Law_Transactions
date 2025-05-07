using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Legal_Law_Transactions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Legal_Law_Transactions.Services;
using System;

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
        public async Task<IActionResult> AddUser(string email, string firstName, string lastName, string role, string password, string status)
        {
            var user = new User
            {
                email = email,
                firstname = firstName,
                lastname = lastName,
                role = role,
                status = status
            };

            user.password = _passwordHasher.HashPassword(user, password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var adminIdString = HttpContext.Session.GetString("UserId"); 
            if (int.TryParse(adminIdString, out int adminId))
            {
                _adminLogService.LogAction(adminId, "Add User", "User", $"Added user {firstName} {lastName} (Role: {role})");
            }

            TempData["SuccessMessage"] = "User added successfully.";
            return RedirectToAction("Users");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUser(int user_id, string email, string firstName, string lastName, string role, string password, string status)
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
            user.status = status;


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
            TempData["SuccessMessage"] = "User updated successfully.";
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
            TempData["SuccessMessage"] = "Case status updated successfully.";

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
            TempData["SuccessMessage"] = "Document status updated successfully.";

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
            TempData["SuccessMessage"] = "License status updated successfully.";

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
            TempData["SuccessMessage"] = "Record status updated successfully.";

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
            TempData["SuccessMessage"] = "Schedule status updated successfully.";

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

        public IActionResult Application()
        {
            var applications = _context.Applications.Include(a => a.User).ToList();
            return View(applications);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateApplication(int application_id, string status, string feedback)
        {
            var application = await _context.Applications.FindAsync(application_id);
            if (application == null)
            {
                return NotFound();
            }

            application.status = status;

            if (status == "Approved")
            {
                var user = await _context.Users.FindAsync(application.user_id);
                if (user != null)
                {
                    if (user.role != "Citizen") 
                    {
                        user.role = "Citizen"; 
                        user.status = "Active"; 
                        _context.Users.Update(user);
                    }
                }
            }
            else if (status == "Rejected")
            {
                application.feedback = feedback;

                var user = await _context.Users.FindAsync(application.user_id);
                if (user != null)
                {
                    user.role = "Applicant";  
                    user.status = "Pending";  
                    _context.Users.Update(user);
                }
            }

            _context.Applications.Update(application);
            await _context.SaveChangesAsync();

            var adminIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(adminIdString, out int adminId))
            {
                _adminLogService.LogAction(adminId, "Update Application", "Application", $"Updated status of application #{application_id} to '{status}'");
            }

            TempData["SuccessMessage"] = "Application status updated successfully.";
            return RedirectToAction("Application");
        }

        public IActionResult ViewApplication(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "application", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }
        public IActionResult ViewDocument(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }
        public IActionResult ViewEvidence(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "evidence", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }
        public IActionResult Evidence(int page = 1)
        {
            int pageSize = 10;
            var totalEvidence = _context.Evidences.Count();
            int totalPages = (int)Math.Ceiling(totalEvidence / (double)pageSize);

            var paginatedEvidences = _context.Evidences
                .OrderBy(e => e.evidence_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["TotalPages"] = totalPages;
            ViewData["Page"] = page;

            return View(paginatedEvidences);
        }
        [HttpPost]
        public IActionResult UpdateEvidenceStatus(int evidence_id, string status)
        {
            var evidence = _context.Evidences.FirstOrDefault(e => e.evidence_id == evidence_id);

            if (evidence == null)
            {
                TempData["ErrorMessage"] = "Evidence not found.";
                return RedirectToAction("Index"); 
            }

            evidence.status = status;

            _context.SaveChanges();

            var adminIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(adminIdString, out int adminId))
            {
                _adminLogService.LogAction(adminId, "Update Evidence", "Application", $"Updated status of case #{evidence_id} to '{status}'");
            }
            TempData["SuccessMessage"] = "Evidence status updated successfully.";
            return RedirectToAction("Evidence"); 
        }

        public IActionResult Logs(int page = 1)
        {
            int pageSize = 10;
            int totalLogs = _context.AdminLogs.Count();
            int totalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);

            var paginatedLogs = _context.AdminLogs
                .OrderByDescending(log => log.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(paginatedLogs);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("Profile", "Admin");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["Error"] = "User session not found.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login", "Account");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.password, CurrentPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Profile", "Admin");
            }

            user.password = _passwordHasher.HashPassword(user, NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile", "Admin");
        }

    }
}
