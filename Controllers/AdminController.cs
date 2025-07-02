using Microsoft.AspNetCore.Mvc;
using KYCIDGenerator.Models;
using Microsoft.EntityFrameworkCore;

namespace KYCIDGenerator.Controllers
{
    public class AdminController : Controller
    {


        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        // Step 1: Show Admin Login Form
        public IActionResult Login()
        {
            return View();
        }

        // Step 2: Receive Admin Login Details → Send OTP
        [HttpPost]
        public IActionResult SendOtp(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            // Clear any existing timer data when starting new session
           // HttpContext.Session.Remove($"otpTimer_{model.Mobile}");


            if (model.AdminName != "admin" || model.Password != "admin123")
            {
                ModelState.AddModelError("", "Invalid Admin Name or Password");
                return View("Login", model);
            }

            string generatedOtp = GenerateRandomOtp();
            TempData["AdminMobile"] = model.Mobile;
            TempData["AdminOtp"] = generatedOtp;
            TempData["AdminName"] = model.AdminName;
            TempData["ResendCount"] = 0;

            // No need to track remaining time server-side anymore
            return RedirectToAction("VerifyOtp");
        }

        // Step 3: Show OTP verification page
        public IActionResult VerifyOtp()
        {
            if (TempData["AdminMobile"] == null || TempData["AdminOtp"] == null)
            {
                return RedirectToAction("Login");
            }

            var model = new OtpVerificationViewModel
            {
                Mobile = TempData["AdminMobile"]?.ToString()
            };

            // Keep only essential data
            TempData.Keep("AdminMobile");
            TempData.Keep("AdminOtp");
            TempData.Keep("AdminName");
            TempData.Keep("ResendCount");

            // Check for success message from resend
            if (TempData["ResendSuccess"] != null)
            {
                ViewBag.ResendSuccess = TempData["ResendSuccess"];
                TempData.Remove("ResendSuccess");
            }

            return View(model);
        }


        public IActionResult Dashboard()
        {
            if (TempData["AdminName"] == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.AdminName = TempData["AdminName"]?.ToString();
            ViewBag.AdminMobile = TempData["AdminMobile"]?.ToString();

            // Keep data for subsequent requests
            TempData.Keep("AdminName");
            TempData.Keep("AdminMobile");

            return View();
        }


        [HttpPost]
        public IActionResult ResendOtp()
        {
            if (TempData["AdminMobile"] == null)
            {
                return Unauthorized();
            }

            var resendCount = TempData["ResendCount"] as int? ?? 0;
            if (resendCount >= 3)
            {
                return StatusCode(429);
            }

            TempData["AdminOtp"] = GenerateRandomOtp();
            TempData["ResendCount"] = resendCount + 1;

            // Remove any existing success message before setting new one
            TempData.Remove("ResendSuccess");
            TempData["ResendSuccess"] = $"New OTP has been sent to {TempData["AdminMobile"]}";

            TempData.Keep("AdminMobile");
            TempData.Keep("AdminName");
            TempData.Keep("ResendCount");

            return Ok();
        }

        [HttpPost]
        public IActionResult VerifyOtp(OtpVerificationViewModel model)
        {
            if (TempData["AdminOtp"] == null || TempData["AdminMobile"] == null)
            {
                return RedirectToAction("Login");
            }

            // Clear the resend success message when verifying OTP
            TempData.Remove("ResendSuccess");

            var correctOtp = TempData["AdminOtp"]?.ToString();
            if (model.Otp == correctOtp)
            {
                TempData.Remove("AdminOtp");
                TempData.Remove("ResendCount");
                return RedirectToAction("Dashboard");
            }

            ModelState.AddModelError("Otp", "Invalid OTP entered. Please try again.");

            TempData.Keep("AdminOtp");
            TempData.Keep("AdminName");
            TempData.Keep("AdminMobile");
            TempData.Keep("ResendCount");

            return View(model);
        }

        private string GenerateRandomOtp()
        {
            // Generate a random 6-digit OTP
            //var random = new Random();
            //return random.Next(100000, 999999).ToString();
            return "123456";
        }

        public async Task<IActionResult> ViewAll(
    int page = 1,
    string? searchQuery = null,
    string? customerTypeFilter = null,
    string? createTypeFilter = null,
    string? statusFilter = null,
    DateTime? createdDateFilter = null,
    DateTime? statusModifiedDateFilter = null)
        {
            int pageSize = 10;

            var baseQuery = _context.KYC_Information.AsQueryable();

            // 🔍 Apply Filters
            if (!string.IsNullOrWhiteSpace(searchQuery))
                baseQuery = baseQuery.Where(k => k.KYC_ID.Contains(searchQuery) || k.Name.Contains(searchQuery));

            if (!string.IsNullOrWhiteSpace(customerTypeFilter))
                baseQuery = baseQuery.Where(k => k.CustomerType == customerTypeFilter);

            if (!string.IsNullOrWhiteSpace(createTypeFilter))
                baseQuery = baseQuery.Where(k => k.CreateType == createTypeFilter);

            if (!string.IsNullOrWhiteSpace(statusFilter))
                baseQuery = baseQuery.Where(k => k.KYC_Status == statusFilter);

            if (createdDateFilter.HasValue)
                baseQuery = baseQuery.Where(k => k.CreatedDate.Date == createdDateFilter.Value.Date);

            if (statusModifiedDateFilter.HasValue)
                baseQuery = baseQuery.Where(k => k.StatusModifiedDate.HasValue &&
                                                 k.StatusModifiedDate.Value.Date == statusModifiedDateFilter.Value.Date);

            // 📊 Pagination
            var totalRecords = await baseQuery.CountAsync();
            var kycs = await baseQuery
                .OrderByDescending(k => k.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new KycListViewModel
            {
                Records = kycs,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                SearchQuery = searchQuery,
                CustomerTypeFilter = customerTypeFilter,
                CreateTypeFilter = createTypeFilter,
                StatusFilter = statusFilter,
                CreatedDateFilter = createdDateFilter,
                StatusModifiedDateFilter = statusModifiedDateFilter
            };

            return View(model);
        }

        public async Task<IActionResult> ViewApproved(
    int page = 1,
    string? searchQuery = null,
    string? customerTypeFilter = null,
    string? createTypeFilter = null,
    DateTime? createdDateFilter = null,
    DateTime? statusModifiedDateFilter = null)
        {
            int pageSize = 10;

            var query = _context.KYC_Information
                .Where(k => k.KYC_Status == "Approved");

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(k => k.KYC_ID.Contains(searchQuery) || k.Name.Contains(searchQuery));

            if (!string.IsNullOrEmpty(customerTypeFilter))
                query = query.Where(k => k.CustomerType == customerTypeFilter);

            if (!string.IsNullOrEmpty(createTypeFilter))
                query = query.Where(k => k.CreateType == createTypeFilter);

            if (createdDateFilter.HasValue)
                query = query.Where(k => k.CreatedDate.Date == createdDateFilter.Value.Date);

            if (statusModifiedDateFilter.HasValue)
                query = query.Where(k => k.StatusModifiedDate.HasValue && k.StatusModifiedDate.Value.Date == statusModifiedDateFilter.Value.Date);

            int totalRecords = await query.CountAsync();

            var records = await query
                .OrderByDescending(k => k.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new KycListViewModel
            {
                Records = records,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                SearchQuery = searchQuery,
                CustomerTypeFilter = customerTypeFilter,
                CreateTypeFilter = createTypeFilter,
                CreatedDateFilter = createdDateFilter,
                StatusModifiedDateFilter = statusModifiedDateFilter
            };

            return View(model);
        }


        public async Task<IActionResult> ViewRejected(
    int page = 1,
    string? searchQuery = null,
    string? customerTypeFilter = null,
    string? createTypeFilter = null,
    DateTime? createdDateFilter = null,
    DateTime? statusModifiedDateFilter = null)
        {
            int pageSize = 10;

            var baseQuery = _context.KYC_Information
                .Where(k => k.KYC_Status == "Rejected");

            if (!string.IsNullOrEmpty(searchQuery))
                baseQuery = baseQuery.Where(k => k.KYC_ID.Contains(searchQuery) || k.Name.Contains(searchQuery));

            if (!string.IsNullOrEmpty(customerTypeFilter))
                baseQuery = baseQuery.Where(k => k.CustomerType == customerTypeFilter);

            if (!string.IsNullOrEmpty(createTypeFilter))
                baseQuery = baseQuery.Where(k => k.CreateType == createTypeFilter);

            if (createdDateFilter.HasValue)
                baseQuery = baseQuery.Where(k => k.CreatedDate.Date == createdDateFilter.Value.Date);

            if (statusModifiedDateFilter.HasValue)
                baseQuery = baseQuery.Where(k => k.StatusModifiedDate.HasValue && k.StatusModifiedDate.Value.Date == statusModifiedDateFilter.Value.Date);

            var totalRecords = await baseQuery.CountAsync();

            var kycs = await baseQuery
                .OrderByDescending(k => k.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get only manual KYC IDs
            var manualKycIds = kycs
                .Where(k => k.CreateType == "Manual")
                .Select(k => k.KYC_ID)
                .ToList();

            // Fetch remarks for those KYCs
            var remarksDict = await _context.Manual_KYCs
                .Where(m => manualKycIds.Contains(m.KYCID))
                .ToDictionaryAsync(m => m.KYCID, m => m.Remarks);

            var viewModels = kycs.Select(k => new KycViewModel
            {
                KYC_ID = k.KYC_ID,
                Name = k.Name,
                CustomerType = k.CustomerType,
                CreateType = k.CreateType,
                KYC_Status = k.KYC_Status,
                CreatedDate = k.CreatedDate,
                StatusModifiedDate = k.StatusModifiedDate,
                RejectedReason = (k.CreateType == "Manual" && remarksDict.TryGetValue(k.KYC_ID, out var remark) && !string.IsNullOrWhiteSpace(remark))
                                 ? remark
                                 : "N/A"
            }).ToList();

            var model = new KycListViewModel2
            {
                Records = viewModels,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                SearchQuery = searchQuery,
                CustomerTypeFilter = customerTypeFilter,
                CreateTypeFilter = createTypeFilter,
                CreatedDateFilter = createdDateFilter,
                StatusModifiedDateFilter = statusModifiedDateFilter
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> ViewPending(
    int page = 1,
    string? searchQuery = null,
    string? customerTypeFilter = null,
    DateTime? createdDateFilter = null)
        {
            int pageSize = 10;

            var baseQuery = _context.KYC_Information
                .Where(k => k.KYC_Status == "Pending");

            if (!string.IsNullOrWhiteSpace(searchQuery))
                baseQuery = baseQuery.Where(k => k.KYC_ID.Contains(searchQuery) || k.Name.Contains(searchQuery));

            if (!string.IsNullOrWhiteSpace(customerTypeFilter))
                baseQuery = baseQuery.Where(k => k.CustomerType == customerTypeFilter);

            if (createdDateFilter.HasValue)
                baseQuery = baseQuery.Where(k => k.CreatedDate.Date == createdDateFilter.Value.Date);

            var totalRecords = await baseQuery.CountAsync();

            var kycs = await baseQuery
                .OrderByDescending(k => k.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new KycListViewModel
            {
                Records = kycs,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                SearchQuery = searchQuery,
                CustomerTypeFilter = customerTypeFilter,
                CreatedDateFilter = createdDateFilter
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> ViewKycDetails(string kycId)
        {
            if (string.IsNullOrWhiteSpace(kycId))
                return NotFound();

            var kyc = await _context.KYC_Information.FirstOrDefaultAsync(k => k.KYC_ID == kycId);
            if (kyc == null)
                return NotFound();

            var manualKyc = await _context.Manual_KYCs.FirstOrDefaultAsync(m => m.KYCID == kycId);

            var model = new KycDetailsViewModel
            {
                Kyc = kyc,
                DocumentName = manualKyc?.DocumentName,
                UploadedFilePath = manualKyc?.UploadedFilePath,
                IsApproved = kyc.KYC_Status == "Approved" || kyc.KYC_Status == "Rejected"
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ApproveKyc(string kycId)
        {
            var kyc = await _context.KYC_Information.FirstOrDefaultAsync(k => k.KYC_ID == kycId);
            if (kyc == null) return NotFound();

            kyc.KYC_Status = "Approved";
            kyc.StatusModifiedDate = DateTime.Now;

            var manual = await _context.Manual_KYCs.FirstOrDefaultAsync(m => m.KYCID == kycId);
            if (manual != null)
            {
                manual.Status = "Approved";
                manual.StatusModifiedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "KYC Approved successfully!";
            return RedirectToAction("ViewKycDetails", new { kycId });
        }

        [HttpGet]
        public IActionResult RejectKycReason(string kycId)
        {
            return View(new KycRejectionViewModel { KycId = kycId });
        }

        [HttpPost]
        public async Task<IActionResult> RejectKycReason(KycRejectionViewModel model)
        {
            if (string.IsNullOrEmpty(model.KycId))
                return NotFound();

            var reason = string.IsNullOrWhiteSpace(model.CustomReason) ? model.SelectedReason : model.CustomReason;

            if (string.IsNullOrWhiteSpace(reason))
            {
                ModelState.AddModelError("", "Please select or enter a reason for rejection.");
                return View(model);
            }

            var kyc = await _context.KYC_Information.FirstOrDefaultAsync(k => k.KYC_ID == model.KycId);
            if (kyc == null)
                return NotFound();

            kyc.KYC_Status = "Rejected";
            kyc.StatusModifiedDate = DateTime.Now;

            var manual = await _context.Manual_KYCs.FirstOrDefaultAsync(m => m.KYCID == model.KycId);
            if (manual != null)
            {
                manual.Status = "Rejected";
                manual.StatusModifiedDate = DateTime.Now;
                manual.Remarks = reason;
            }

            await _context.SaveChangesAsync();

            TempData["RejectedMessage"] = "KYC has been rejected successfully.";
            return RedirectToAction("RejectKycConfirmation", new { kycId = model.KycId });
        }

        [HttpGet]
        public IActionResult RejectKycConfirmation(string kycId)
        {
            if (string.IsNullOrEmpty(kycId))
                return NotFound();

            ViewBag.KycId = kycId;
            return View();
        }


    }
}
