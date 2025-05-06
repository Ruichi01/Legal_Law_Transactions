using Legal_Law_Transactions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;

namespace Legal_Law_Transactions.Controllers
{
    [Authorize(Roles = "Enforcer")]
    public class EnforcerController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public EnforcerController(ApplicationDbContext context, IWebHostEnvironment environment, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _environment = environment;
            _passwordHasher = passwordHasher;

        }

        public IActionResult Dashboard()
        {
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
        public IActionResult AddRecord(int user_id, int case_id, string offense_type, DateTime date_of_offense, string status)
        {
            var record = new Record
            {
                user_id = user_id,
                case_id = case_id,
                offense_type = offense_type,
                date_of_offense = date_of_offense,
                status = "Convicted",
                last_updated = DateTime.Now
            };

            _context.Records.Add(record);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Record uploaded successfully.";
            return RedirectToAction("Records");
        }

        [HttpPost]
        public IActionResult UpdateRecord(int record_id, int user_id, int case_id, string offense_type, DateTime date_of_offense, string status)
        {
            var record = _context.Records.FirstOrDefault(r => r.record_id == record_id);
            if (record != null)
            {
                record.user_id = user_id;
                record.case_id = case_id;
                record.offense_type = offense_type;
                record.date_of_offense = date_of_offense;
                record.status = status;
                record.last_updated = DateTime.Now;

                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = "Record updated successfully.";
            return RedirectToAction("Records");
        }

        [HttpGet]
        public IActionResult Records(int page = 1)
        {
            int pageSize = 10;
            var records = _context.Records
                .OrderBy(r => r.record_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Users = _context.Users.ToList();
            ViewBag.Cases = _context.Cases.ToList();

            return View(records);
        }

        public IActionResult LicenseVerification(int page = 1)
        {
            int pageSize = 10;
            var totalLicenses = _context.Licenses.Count();
            int totalPages = (int)Math.Ceiling(totalLicenses / (double)pageSize);

            var licenses = _context.Licenses
                .OrderBy(l => l.license_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(l => l.User) 
                .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return View(licenses);
        }

        [HttpPost]
        public IActionResult UpdateLicense(int license_id, string type, DateTime issue_date, DateTime expiry_date, string status)
        {
            var license = _context.Licenses.FirstOrDefault(l => l.license_id == license_id);
            if (license != null)
            {
                license.type = type;
                license.issue_date = issue_date;
                license.expiry_date = expiry_date;
                license.status = status;

                _context.SaveChanges();
            }
            TempData["SuccessMessage"] = "License updated successfully.";
            return RedirectToAction("LicenseVerification");
        }


        [HttpPost]
        public IActionResult AddLicense(int user_id, string type, DateTime issue_date, DateTime expiry_date, string status)
        {
            var license = new License
            {
                user_id = user_id,
                type = type,
                issue_date = issue_date,
                expiry_date = expiry_date,
                status = status
            };

            _context.Licenses.Add(license);
            _context.SaveChanges();

            return RedirectToAction("LicenseVerification");
        }
        public IActionResult CaseManagement(int page = 1)
        {
            int pageSize = 10;
            var totalCases = _context.Cases.Count();
            int totalPages = (int)Math.Ceiling(totalCases / (double)pageSize);

            var cases = _context.Cases
                .OrderBy(c => c.case_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            ViewBag.Users = _context.Users.ToList();

            return View(cases);
        }


        [HttpPost]
        public IActionResult UpdateCase(int case_id, int assigned_to_lawyer, int assigned_to_citizen, string status, DateTime court_date)
        {
            var caseItem = _context.Cases.FirstOrDefault(c => c.case_id == case_id);
            if (caseItem != null)
            {
                caseItem.assigned_to_lawyer = assigned_to_lawyer;
                caseItem.assigned_to_citizen = assigned_to_citizen;
                caseItem.status = status;
                caseItem.court_date = court_date;

                _context.SaveChanges();
            }

            return RedirectToAction("CaseManagement");
        }

        [HttpPost]
        public IActionResult AddCase(int assigned_to_lawyer, int assigned_to_citizen, string description, DateTime court_date)
        {
            var lawyerExists = _context.Users.Any(u => u.user_id == assigned_to_lawyer && u.role == "Lawyer");
            var citizenExists = _context.Users.Any(u => u.user_id == assigned_to_citizen && u.role == "Citizen");

            if (!lawyerExists || !citizenExists)
            {
                TempData["ErrorMessage"] = "Assigned Lawyer or Citizen does not exist.";
                return RedirectToAction("CaseManagement");
            }

            int nextId = (_context.Cases.OrderByDescending(c => c.case_id).FirstOrDefault()?.case_id ?? 0) + 1;
            string caseNumber = $"CASE-{nextId.ToString("D4")}";

            var newCase = new Case
            {
                assigned_to_lawyer = assigned_to_lawyer,
                assigned_to_citizen = assigned_to_citizen,
                case_number = caseNumber,
                description = description,
                status = "Filed",
                court_date = court_date,
                created_at = DateTime.Now,
                user_id = assigned_to_citizen 
            };

            _context.Cases.Add(newCase);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Case added successfully.";
            return RedirectToAction("CaseManagement");
        }



        public IActionResult EvidenceUpload()
        {
            ViewBag.Users = _context.Users.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddDocument(IFormCollection form, IFormFile file)
        {
            var userId = int.Parse(form["user_id"]);
            var title = form["title"];
            var content = form["content"];
            var notarized = form["notarized"];

            string filePath = null;

            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "evidence");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                filePath = Path.Combine("evidence", uniqueFileName);

                using (var fileStream = new FileStream(Path.Combine(_environment.WebRootPath, filePath), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }

            var newDoc = new Document
            {
                user_id = userId,
                title = title,
                content = content,
                notarized = "Undetermined",
                file_path = filePath,
                created_at = DateTime.Now
            };

            _context.Documents.Add(newDoc);
            await _context.SaveChangesAsync();

            return RedirectToAction("EvidenceUpload");
        }

        public IActionResult EvidenceRecords(int page = 1)
        {
            int pageSize = 10;
            var totalDocs = _context.Documents.Count();
            int totalPages = (int)Math.Ceiling(totalDocs / (double)pageSize);

            var documents = _context.Documents
                .OrderBy(d => d.document_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.User) 
                .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return View(documents);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDocument(IFormCollection form, IFormFile file)
        {
            var docId = int.Parse(form["document_id"]);
            var doc = await _context.Documents.FindAsync(docId);
            if (doc == null)
            {
                return NotFound();
            }

            doc.user_id = int.Parse(form["user_id"]);
            doc.title = form["title"];
            doc.content = form["content"];
            doc.notarized = form["notarized"];

            if (file != null && file.Length > 0)
            {
                if (!string.IsNullOrEmpty(doc.file_path))
                {
                    string oldPath = Path.Combine(_environment.WebRootPath, doc.file_path);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "evidence");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                string filePath = Path.Combine("evidence", uniqueFileName);

                using (var fileStream = new FileStream(Path.Combine(_environment.WebRootPath, filePath), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                doc.file_path = filePath;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("EvidenceUpload");
        }

        public JsonResult GetCasesForUser(int userId)
        {
            var cases = _context.Cases
                                .Where(c => c.user_id == userId)
                                .Select(c => new {
                                    c.case_id,
                                    c.case_number,
                                    c.description
                                })
                                .ToList();
            return Json(cases);
        }

        public IActionResult ViewEvidence(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "evidence", fileName);

            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", fileName);  
            }

            return NotFound();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("Profile", "Enforcer");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["Error"] = "User session not found.";
                return RedirectToAction("Login", "Enforcer");
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
                return RedirectToAction("Profile", "Enforcer");
            }

            user.password = _passwordHasher.HashPassword(user, NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile", "Enforcer");
        }

    }
}