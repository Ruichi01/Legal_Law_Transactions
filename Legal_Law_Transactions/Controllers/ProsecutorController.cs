using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Legal_Law_Transactions.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dropbox.Sign.Api;
using Dropbox.Sign.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace Legal_Law_Transactions.Controllers
{
    [Authorize(Roles = "Prosecutor")]
    public class ProsecutorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPasswordHasher<User> _passwordHasher;


        public ProsecutorController(ApplicationDbContext context, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _passwordHasher = passwordHasher;

        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Cases()
        {
            var cases = _context.Cases.ToList();
            return View(cases);
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
        public IActionResult Documents(int page = 1)
        {
            int pageSize = 10;

            var totalDocuments = _context.Documents.Count();

            int totalPages = (int)Math.Ceiling(totalDocuments / (double)pageSize);

            var documents = _context.Documents
            .OrderByDescending(d => d.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            ViewBag.Lawyers = _context.Users.Where(u => u.role == "Lawyer").ToList();

            return View(documents);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignLawyerToDocument(int documentId, int assignedToLawyer, string notarized, IFormFile? newFile)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction("Documents");
            }

            document.assigned_to_lawyer = assignedToLawyer;
            document.notarized = notarized;

            if (newFile != null)
            {
                var extension = Path.GetExtension(newFile.FileName).ToLowerInvariant();
                if (extension != ".pdf" || newFile.ContentType != "application/pdf")
                {
                    TempData["ErrorMessage"] = "Only PDF files are allowed.";
                    return RedirectToAction("Documents");
                }

                if (!string.IsNullOrEmpty(document.file_path))
                {
                    var oldPath = Path.Combine("wwwroot", document.file_path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var fileName = Guid.NewGuid() + ".pdf";
                var savePath = Path.Combine("wwwroot/documents", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await newFile.CopyToAsync(stream);
                }

                document.file_path = "/documents/" + fileName;
            }

            _context.Documents.Update(document);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Document updated successfully.";
            return RedirectToAction("Documents");
        }

        [HttpPost]
        public async Task<IActionResult> SignDocument(int documentId)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.document_id == documentId);
            if (document == null)
                return NotFound("Document not found.");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", Path.GetFileName(document.file_path));
            if (!System.IO.File.Exists(filePath))
                return NotFound("File does not exist.");

            var sessionEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(sessionEmail))
                return Unauthorized("No session email found.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == sessionEmail);
            if (user == null)
                return Unauthorized("User not found in the database.");

            var signerEmail = user.email;
            var signerName = $"{user.firstname} {user.lastname}";

            var lawyer = await _context.Users.FirstOrDefaultAsync(u => u.user_id == document.assigned_to_lawyer);
            if (lawyer == null)
                return NotFound("Assigned lawyer not found.");

            var secondSignerEmail = lawyer.email;
            var secondSignerName = $"{lawyer.firstname} {lawyer.lastname}";

            var apiKey = _configuration["DropboxSign:ApiKey"];
            var config = new Dropbox.Sign.Client.Configuration { Username = apiKey };
            var signatureApi = new SignatureRequestApi(config);

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var signer1 = new SubSignatureRequestSigner(emailAddress: signerEmail, name: signerName);
            var signer2 = new SubSignatureRequestSigner(emailAddress: secondSignerEmail, name: secondSignerName);

            var request = new SignatureRequestSendRequest(
                title: document.title,
                subject: "Please sign this legal document",
                signers: new List<SubSignatureRequestSigner> { signer1, signer2 },
                files: new List<Stream> { fileStream }
            )
            {
                TestMode = true
            };

            var result = await signatureApi.SignatureRequestSendAsync(request);

            if (result?.SignatureRequest?.SignatureRequestId == null)
                return StatusCode(500, "Failed to send signature request.");

            var newSignature = new Signature
            {
                user_id = user.user_id,
                document_id = documentId,
                signature_hash = result.SignatureRequest.SignatureRequestId,
                signed_at = DateTime.Now
            };

            _context.Signatures.Add(newSignature);
            await _context.SaveChangesAsync();

            return RedirectToAction("Documents");
        }

        public IActionResult Evidences()
        {
            var evidences = _context.Records.ToList();
            return View(evidences);
        }

        public IActionResult Schedules(int? selectedCaseId)
        {
            var schedules = _context.Schedules.ToList();

            var scheduledCaseIds = _context.Schedules.Select(s => s.case_id).ToList();
            var unscheduledCases = _context.Cases
                .Where(c => !scheduledCaseIds.Contains(c.case_id))
                .ToList();

            ViewBag.CaseList = new SelectList(unscheduledCases, "case_id", "case_number", selectedCaseId);
            ViewBag.SelectedCaseId = selectedCaseId;
            ViewBag.ShowAddScheduleModal = selectedCaseId.HasValue;

            return View(schedules);
        }


        [HttpPost]
        public IActionResult AddSchedule(int case_id, int assigned_to_lawyer, int assigned_to_citizen, string courtroom_number, string status, DateTime court_date)
        {
            if (_context.Schedules.Any(s => s.case_id == case_id))
            {
                TempData["ErrorMessage"] = "A schedule already exists for this case.";
                return RedirectToAction("Schedules");
            }

            var newSchedule = new Schedule
            {
                case_id = case_id,
                assigned_to_lawyer = assigned_to_lawyer,
                assigned_to_citizen = assigned_to_citizen,
                courtroom_number = courtroom_number,
                status = "Scheduled",
                court_date = court_date
            };

            _context.Schedules.Add(newSchedule);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Schedule added successfully.";
            return RedirectToAction("Schedules");
        }


        [HttpPost]
        public IActionResult EditSchedule(int schedule_id, int case_id, int assigned_to_lawyer, int assigned_to_citizen, string courtroom_number, string status, DateTime court_date)
        {
            var schedule = _context.Schedules.FirstOrDefault(s => s.schedule_id == schedule_id);
            if (schedule != null)
            {
                schedule.case_id = case_id;
                schedule.assigned_to_lawyer = assigned_to_lawyer;
                schedule.assigned_to_citizen = assigned_to_citizen;
                schedule.courtroom_number = courtroom_number;
                schedule.status = status;
                schedule.court_date = court_date;

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Schedule updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Schedule not found.";
            }

            return RedirectToAction("Schedules");
        }
        [HttpGet]
        public IActionResult GetParticipantsForCase(int caseId)
        {
            var selectedCase = _context.Cases.FirstOrDefault(c => c.case_id == caseId);
            if (selectedCase == null)
                return NotFound();

            var lawyer = _context.Users.FirstOrDefault(u => u.user_id == selectedCase.assigned_to_lawyer);
            var citizen = _context.Users.FirstOrDefault(u => u.user_id == selectedCase.assigned_to_citizen);

            return Json(new
            {
                lawyer = lawyer == null ? null : new { lawyer.user_id, lawyer.firstname, lawyer.lastname },
                citizen = citizen == null ? null : new { citizen.user_id, citizen.firstname, citizen.lastname }
            });
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
            TempData["SuccessMessage"] = "Case updated successfully.";

            return RedirectToAction("Cases");
        }

        [HttpPost]
        public IActionResult AddCase(int assigned_to_lawyer, int assigned_to_citizen, string description, DateTime court_date)
        {
            var lawyerExists = _context.Users.Any(u => u.user_id == assigned_to_lawyer && u.role == "Lawyer");
            var citizenExists = _context.Users.Any(u => u.user_id == assigned_to_citizen && u.role == "Citizen");

            if (!lawyerExists || !citizenExists)
            {
                TempData["ErrorMessage"] = "Assigned Lawyer or Citizen does not exist.";
                return RedirectToAction("Cases");
            }

            var currentUserEmail = User.Identity.Name;
            var prosecutorUser = _context.Users.FirstOrDefault(u => u.email == currentUserEmail);

            if (prosecutorUser == null)
            {
                TempData["ErrorMessage"] = "Unable to identify the logged-in prosecutor.";
                return RedirectToAction("Cases");
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
                user_id = prosecutorUser.user_id
            };

            _context.Cases.Add(newCase);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Case added successfully.";
            return RedirectToAction("Cases");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("Profile", "Prosecutor");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["Error"] = "User session not found.";
                return RedirectToAction("Login", "Prosecutor");
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
                return RedirectToAction("Profile", "Prosecutor");
            }

            user.password = _passwordHasher.HashPassword(user, NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile", "Prosecutor");
        }
    }
}