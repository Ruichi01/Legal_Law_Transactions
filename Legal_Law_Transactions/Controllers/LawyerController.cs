using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Legal_Law_Transactions.Models;
using System.Linq;
using Dropbox.Sign;
using Dropbox.Sign.Model;
using Dropbox.Sign.Client;
using Dropbox.Sign.Api;
using Microsoft.Extensions.Configuration;

namespace Legal_Law_Transactions.Controllers
{
    [Authorize(Roles = "Lawyer")]
    public class LawyerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;


        public LawyerController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

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

        public IActionResult MyCases(int page = 1)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);

            if (currentUser == null)
                return Unauthorized();

            var myCases = _context.Cases
                .Where(c => c.user_id == currentUser.user_id)
                .OrderBy(c => c.case_id)
                .Skip((page - 1) * 10)
                .Take(10)
                .ToList();

            int totalCases = _context.Cases.Count(c => c.user_id == currentUser.user_id);
            int totalPages = (int)Math.Ceiling(totalCases / 10.0);

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(myCases);
        }

        [HttpPost]
        public IActionResult UpdateCaseStatus(int caseId, string status)
        {
            var caseItem = _context.Cases.FirstOrDefault(c => c.case_id == caseId);
            if (caseItem != null)
            {
                caseItem.status = status;
                _context.SaveChanges();
            }
            return RedirectToAction("MyCases");
        }

        public IActionResult Schedules(int page = 1)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);

            if (currentUser == null)
                return Unauthorized();

            var schedules = _context.Schedules
                .Include(s => s.Case)
                .Where(s => s.Case.user_id == currentUser.user_id)
                .OrderBy(s => s.court_date)
                .Skip((page - 1) * 10)
                .Take(10)
                .ToList();

            int totalSchedules = _context.Schedules
                .Include(s => s.Case)
                .Count(s => s.Case.user_id == currentUser.user_id);
            int totalPages = (int)Math.Ceiling(totalSchedules / 10.0);

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(schedules);
        }

        public IActionResult Evidences(int page = 1)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);

            if (currentUser == null)
                return Unauthorized();

            var evidences = _context.Evidences
                .Where(e => e.user_id == currentUser.user_id)
                .OrderByDescending(e => e.created_at)
                .Skip((page - 1) * 10)
                .Take(10)
                .ToList();

            int totalEvidences = _context.Evidences.Count(e => e.user_id == currentUser.user_id);
            int totalPages = (int)Math.Ceiling(totalEvidences / 10.0);

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(evidences);
        }
        [HttpPost]
        public IActionResult EditEvidenceStatus(int evidence_id, string status)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);

            if (currentUser == null)
                return Unauthorized();

            var evidence = _context.Evidences.FirstOrDefault(e =>
                e.evidence_id == evidence_id &&
                (e.assigned_to_lawyer == currentUser.user_id || e.assigned_to_citizen == currentUser.user_id)
            );

            if (evidence != null)
            {
                // Only allow these specific statuses
                if (new[] { "Pending", "Analyzing", "Validated" }.Contains(status))
                {
                    evidence.status = status;
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Evidences");
        }


        public IActionResult Documents(int page = 1)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var currentUser = _context.Users.FirstOrDefault(u => u.email == email);

            if (currentUser == null)
                return Unauthorized();

            var documents = _context.Documents
                .Where(d => d.user_id == currentUser.user_id)
                .OrderByDescending(d => d.created_at)
                .Skip((page - 1) * 10)
                .Take(10)
                .ToList();

            int totalDocuments = _context.Documents.Count(d => d.user_id == currentUser.user_id);
            int totalPages = (int)Math.Ceiling(totalDocuments / 10.0);

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(documents);
        }
       
        public IActionResult ViewDocument(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", fileName);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
        public IActionResult ViewEvidence(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "evidence", fileName);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
