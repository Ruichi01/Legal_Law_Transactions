﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Legal_Law_Transactions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace Legal_Law_Transactions.Controllers
{
    [Authorize(Roles = "Citizen")]
    public class CitizenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPasswordHasher<User> _passwordHasher;


        public CitizenController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            ViewBag.FullName = $"{currentUser.firstname} {currentUser.lastname}";
            ViewBag.Email = currentUser.email;

            return View();
        }

        public IActionResult MyRecords(int page = 1)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            int pageSize = 10;
            var totalRecords = _context.Records.Where(r => r.user_id == currentUser.user_id).Count();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var records = _context.Records
                .Where(r => r.user_id == currentUser.user_id)
                .OrderBy(r => r.record_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.Case)
                .ToList();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(records);
        }

        public IActionResult MyCases(int page = 1)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            int pageSize = 10;
            var totalCases = _context.Cases.Where(c => c.user_id == currentUser.user_id).Count();
            int totalPages = (int)Math.Ceiling(totalCases / (double)pageSize);

            var cases = _context.Cases
                .Where(c => c.user_id == currentUser.user_id)
                .OrderBy(c => c.case_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(cases);
        }

        public IActionResult MyLicense()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var license = _context.Licenses.FirstOrDefault(l => l.user_id == currentUser.user_id);
            return View(license);
        }

        [HttpPost]
        public async Task<IActionResult> AddLicense(string type, string status, DateTime issue_date, DateTime expiry_date, IFormFile licenseImage)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var existing = _context.Licenses.FirstOrDefault(l => l.user_id == currentUser.user_id);
            if (existing != null)
            {
                existing.type = type;
                existing.status = expiry_date < DateTime.Today ? "Inactive" : "Pending";

                existing.issue_date = issue_date;
                existing.expiry_date = expiry_date;

                if (licenseImage != null)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(licenseImage.FileName);
                    var path = Path.Combine("wwwroot/images", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await licenseImage.CopyToAsync(stream);
                    }
                    existing.license_image = "/images/" + fileName;
                }

                _context.Licenses.Update(existing);
            }
            else
            {
                var license = new License
                {
                    user_id = currentUser.user_id,
                    type = type,
                    status = expiry_date < DateTime.Today ? "Inactive" : "Pending",
                    issue_date = issue_date,
                    expiry_date = expiry_date
                };
                if (licenseImage != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var allowedContentTypes = new[] { "image/jpeg", "image/png" };
                    var extension = Path.GetExtension(licenseImage.FileName).ToLowerInvariant();
                    var contentType = licenseImage.ContentType;

                    if (!allowedExtensions.Contains(extension) || !allowedContentTypes.Contains(contentType))
                    {
                        TempData["ErrorMessage"] = "Only JPG and PNG images are allowed.";
                        return RedirectToAction("MyLicense");
                    }


                    var fileName = Guid.NewGuid() + extension;
                    var path = Path.Combine("wwwroot/images", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await licenseImage.CopyToAsync(stream);
                    }

                    license.license_image = "/images/" + fileName;
                }


                _context.Licenses.Add(license);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = existing != null ? "License updated successfully." : "License added successfully.";
            return RedirectToAction("MyLicense");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLicense(int license_id, string type, string status, DateTime issue_date, DateTime expiry_date, IFormFile? licenseImage)
        {
            var license = await _context.Licenses.FindAsync(license_id);
            if (license == null) return NotFound();

            license.type = type;
            license.status = expiry_date < DateTime.Today ? "Inactive" : "Pending";
            license.issue_date = issue_date;
            license.expiry_date = expiry_date;


            if (licenseImage != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var allowedContentTypes = new[] { "image/jpeg", "image/png" };
                var extension = Path.GetExtension(licenseImage.FileName).ToLowerInvariant();
                var contentType = licenseImage.ContentType;

                if (!allowedExtensions.Contains(extension) || !allowedContentTypes.Contains(contentType))
                {
                    TempData["ErrorMessage"] = "Only JPG and PNG images are allowed.";
                    return RedirectToAction("MyLicense");
                }

                var fileName = Guid.NewGuid() + extension;
                var path = Path.Combine("wwwroot/images", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await licenseImage.CopyToAsync(stream);
                }

                license.license_image = "/images/" + fileName;
            }

            _context.Licenses.Update(license);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "License updated successfully.";
            return RedirectToAction("MyLicense");
        }

        [HttpPost]
        public async Task<IActionResult> UploadEvidence(IFormFile file, int case_id, string description)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "evidence", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var assignedLawyer = _context.Schedules
                    .Where(s => s.case_id == case_id)
                    .OrderByDescending(s => s.court_date)
                    .Select(s => s.assigned_to_lawyer)
                    .FirstOrDefault();

                var evidence = new Evidence
                {
                    case_id = case_id,
                    user_id = currentUser.user_id,
                    assigned_to_citizen = currentUser.user_id,
                    assigned_to_lawyer = assignedLawyer,
                    file_path = "/evidence/" + fileName,
                    description = description,
                    status = "Pending",
                    created_at = DateTime.Now
                };

                _context.Evidences.Add(evidence);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Evidence uploaded successfully.";
                return RedirectToAction("MyDocuments");
            }

            TempData["Error"] = "File upload failed.";
            return RedirectToAction("MyDocuments");
        }

        public IActionResult ViewEvidence(string fileName)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return Unauthorized();

            var evidence = _context.Evidences
                .FirstOrDefault(e => e.file_path.Contains(fileName) && e.user_id == currentUser.user_id);

            if (evidence == null) return NotFound();

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "evidence", fileName);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }

        public IActionResult MyDocuments()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            int pageSize = 10;
            int currentPage = string.IsNullOrEmpty(Request.Query["page"]) ? 1 : int.Parse(Request.Query["page"]);

            var totalCount = _context.Evidences.Count(e => e.user_id == currentUser.user_id);
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = totalPages;

            var myCases = _context.Cases.Where(c => c.user_id == currentUser.user_id).ToList();

            return View(myCases);
        }


        public IActionResult RequestNotarization(int page = 1)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            int pageSize = 10;
            var totalDocuments = _context.Documents.Where(d => d.user_id == currentUser.user_id).Count();
            int totalPages = (int)Math.Ceiling(totalDocuments / (double)pageSize);

            var documents = _context.Documents
                .Where(d => d.user_id == currentUser.user_id)
                .OrderByDescending(d => d.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(documents);

        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(string title, string content, IFormFile file)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var newDocument = new Document
                {
                    title = title,
                    content = content,
                    notarized = "No",
                    created_at = DateTime.Now,
                    last_modified = DateTime.Now,
                    user_id = currentUser.user_id,
                    assigned_to_citizen = currentUser.user_id,
                    file_path = "/documents/" + fileName
                };

                _context.Documents.Add(newDocument);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("RequestNotarization");
        }

        public IActionResult ViewDocument(string fileName)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return Unauthorized();

            var document = _context.Documents
                .FirstOrDefault(d => d.file_path.Contains(fileName) && d.user_id == currentUser.user_id);
            if (document == null) return NotFound();

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "documents", fileName);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }

        public IActionResult MySchedule()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            int pageSize = 10;
            int currentPage = string.IsNullOrEmpty(Request.Query["page"]) ? 1 : int.Parse(Request.Query["page"]);

            var myCaseIds = _context.Cases
                .Where(c => c.user_id == currentUser.user_id)
                .Select(c => c.case_id)
                .ToList();

            var totalCount = _context.Schedules.Count(s => myCaseIds.Contains(s.case_id));
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var schedules = _context.Schedules
                .Where(s => myCaseIds.Contains(s.case_id))
                .Include(s => s.Case)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = totalPages;

            return View(schedules);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("Profile", "Citizen");
            }

            var userIdString = HttpContext.Session.GetString("UserId"); 
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["Error"] = "User session not found.";
                return RedirectToAction("Login", "Citizen");
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
                return RedirectToAction("Profile", "Citizen"); 
            }

            user.password = _passwordHasher.HashPassword(user, NewPassword);
            _context.Users.Update(user);  
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile", "Citizen");  
        }


    }
}
