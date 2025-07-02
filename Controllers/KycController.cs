using Microsoft.AspNetCore.Mvc;
//using KYCIDGenerator.Data;
using KYCIDGenerator.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.IO;
using System.Security.Cryptography;


public class KycController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
  

    public KycController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // Show Home
    public IActionResult Home() => View(new KycHomeViewModel());


    [HttpPost]
    public IActionResult SelectKycType(KycHomeViewModel model)
    {
        // Just return the same view with the selected KYC type persisted
        return View("Home", model);
    }


    // POST: Search by KYC ID
    //[HttpPost]
    //public async Task<IActionResult> SearchKyc(KycHomeViewModel model)
    //{
    //    if (string.IsNullOrWhiteSpace(model.KycIdSearch))
    //    {
    //        ModelState.AddModelError("", "Enter KYC ID.");
    //        return View("Home", model);
    //    }

    //    var kyc = await _context.KYC_Information.FindAsync(model.KycIdSearch);
    //    if (kyc == null)
    //    {
    //        ModelState.AddModelError("", "KYC not found.");
    //        return View("Home", model);
    //    }

    //    return View("KycFound", kyc);
    //}
    [HttpPost]
    public async Task<IActionResult> SearchKyc(KycHomeViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.KycIdSearch))
        {
            ModelState.AddModelError("", "Enter KYC ID.");
            return View("Home", model);
        }

        var kyc = await _context.KYC_Information
            .Where(x => x.KYC_ID == model.KycIdSearch && x.CustomerType == model.KycType)
            .FirstOrDefaultAsync();

        if (kyc == null)
        {
            ModelState.AddModelError("", "KYC not found.");
            return View("Home", model);
        }

        return View("KycFound", kyc);
    }





    // POST: Proceed from card
    [HttpPost]
    public IActionResult Proceed(string kycId)
    {
        var kyc = _context.KYC_Information.Find(kycId);
        if (kyc == null) return RedirectToAction("Home");

        switch (kyc.KYC_Status)
        {
            case "Approved":
                return RedirectToAction("ThankYou", new { kycId });
            case "Pending":
                return RedirectToAction("Pending", new { kycId });
            case "Rejected":
                return RedirectToAction("Rejected", new { kycId });
            default:
                return RedirectToAction("Home");
        }
    }

    public IActionResult ThankYou(string kycId) => View("ThankYou", kycId);
    public IActionResult Pending(string kycId) => View("Pending", kycId);
    public async Task<IActionResult> Rejected(string kycId)
    {
        var reason = await _context.Manual_KYCs
            .Where(m => m.KYCID == kycId)
            .Select(m => m.Remarks)
            .FirstOrDefaultAsync();

        ViewBag.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Reason not specified." : reason;

        return View(model: kycId);
    }




    [HttpGet]
    public IActionResult AddIndividual()
    {
        var model = new IndividualKycViewModel();
        return View(model);
    }

    [HttpGet]
    public IActionResult AddCorporate()
    {
        var model = new CorporateKycViewModel();
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> AddIndividual(IndividualKycViewModel model, string? overrideAction, string? isMethodSwitch)
    {
        // 🔁 Handle method switch UI refresh (from PAN / Aadhaar / Manual buttons)
        if (isMethodSwitch == "true")
        {
            ModelState.Clear();
            return View(model);
        }

        // 🔒 Consent auto-set if overrideAction is createNew
        if (overrideAction == "createNew")
        {
            model.Consent = true;
        }
        else if (!ModelState.IsValid || !model.Consent)
        {
            if (!model.Consent)
                ModelState.AddModelError("", "Consent is required.");
            return View(model);
        }

        // 🔎 Check for existing record (PAN / Aadhaar)
        KYC_Information? existing = null;
        if (model.Method == "PAN")
        {
            existing = await _context.KYC_Information
                .Where(x => x.PAN_Number == model.PAN)
                .OrderByDescending(x => x.KYC_ID)
                .FirstOrDefaultAsync();
        }
        else if (model.Method == "Adhaar")
        {
            existing = await _context.KYC_Information
                .Where(x => x.AdhaarNumber == model.Adhaar)
                .OrderByDescending(x => x.KYC_ID)
                .FirstOrDefaultAsync();
        }

        if (existing != null && overrideAction != "createNew")
        {
            TempData["ExistingKYCId"] = existing.KYC_ID;
            TempData["Method"] = model.Method;
            TempData["PAN"] = model.PAN;
            TempData["Adhaar"] = model.Adhaar;
            TempData["Mobile"] = model.Mobile;
            return RedirectToAction("ExistingKycChoice");
        }

        // 🆕 Generate new KYC ID
        //string newKycId;
        //do
        //{
        //    newKycId = "KYC" + new Random().Next(1000, 9999);
        //} while (await _context.KYC_Information.AnyAsync(x => x.KYC_ID == newKycId));
        // 🆕 Generate new KYC ID (9-character alphanumeric like "EL5EURFUKJ")
        string newKycId;
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; // All uppercase letters and numbers

        do
        {
            newKycId = new string(Enumerable.Repeat(chars, 9)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        } while (await _context.KYC_Information.AnyAsync(x => x.KYC_ID == newKycId));

        // 📝 Prepare base KYC record
        var newKyc = new KYC_Information
        {
            KYC_ID = newKycId,
            Mobile = model.Mobile,
            CustomerType = "Individual",
            Consent = model.Consent ? "Yes" : "No",
            CreatedDate = DateTime.Now,
            StatusModifiedDate = DateTime.Now,
            KYC_Status = "Pending"
        };

        // 🔄 Populate via PAN
        if (model.Method == "PAN")
        {
            var record = await _context.IndividualSamples.FirstOrDefaultAsync(x => x.PAN == model.PAN);
            if (record == null)
            {
                ModelState.AddModelError("", "No KYC data found for the entered PAN.");
                return View(model);
            }

            newKyc.Name = record.Name;
            newKyc.Email = record.Email;
            newKyc.Address1 = record.Address1;
            newKyc.Address2 = record.Address2;
            newKyc.City = record.City;
            newKyc.State = record.State;
            newKyc.DOB = record.DOB;
            newKyc.Pin_Code = record.PinCode;
            newKyc.PAN_Number = record.PAN;
            newKyc.AdhaarNumber = record.Adhaar;
            newKyc.KYC_Status = "Approved";
            newKyc.StatusModifiedDate = DateTime.Now;
            newKyc.CreateType = "Auto";
        }
        // 🔄 Populate via Aadhaar
        else if (model.Method == "Adhaar")
        {
            var record = await _context.IndividualSamples.FirstOrDefaultAsync(x => x.Adhaar == model.Adhaar);
            if (record == null)
            {
                ModelState.AddModelError("", "No KYC data found for the entered Aadhaar.");
                return View(model);
            }

            newKyc.Name = record.Name;
            newKyc.Email = record.Email;
            newKyc.Address1 = record.Address1;
            newKyc.Address2 = record.Address2;
            newKyc.City = record.City;
            newKyc.State = record.State;
            newKyc.DOB = record.DOB;
            newKyc.Pin_Code = record.PinCode;
            newKyc.PAN_Number = record.PAN;
            newKyc.AdhaarNumber = record.Adhaar;
            newKyc.KYC_Status = "Approved";
            newKyc.StatusModifiedDate = DateTime.Now;
            newKyc.CreateType = "Auto";
        }
        // 🧾 Manual Entry + Upload
        else if (model.Method == "Manual")
        {
            newKyc.Name = model.Name;
            newKyc.Email = model.Email;
            newKyc.Address1 = model.Address1;
            newKyc.Address2 = model.Address2;
            newKyc.City = model.City;
            newKyc.State = model.State;
            newKyc.DOB = model.DOB;
            newKyc.Pin_Code = model.PinCode;
            newKyc.KYC_Status = "Pending";
            newKyc.StatusModifiedDate = DateTime.Now;
            newKyc.CreateType = "Manual";

            // 📁 Upload document
            string? filePath = null;
            if (model.UploadedDocument != null && model.UploadedDocument.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ManualDocs");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                string fileName = $"{Guid.NewGuid()}_{model.UploadedDocument.FileName}";
                string fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.UploadedDocument.CopyToAsync(stream);
                }

                filePath = $"/ManualDocs/{fileName}";
            }

            // 🗃 Add to Manual_KYCs
            var manualKyc = new ManualKyc
            {
                KYCID = newKycId,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                StatusModifiedDate = DateTime.Now,
                DocumentName = model.SelectedDocumentType,
                UploadedFilePath = filePath,
                CustomerType = "Individual"
            };

            switch (model.SelectedDocumentType)
            {
                case "PAN": manualKyc.PAN = model.DocumentNumber; break;
                case "ADHAAR": manualKyc.ADHAAR = model.DocumentNumber; break;
                case "LICENCE": manualKyc.LICENCE = model.DocumentNumber; break;
                case "PASSPORT": manualKyc.PASSPORT = model.DocumentNumber; break;
            }

            _context.Manual_KYCs.Add(manualKyc);
        }

        // ✅ Save KYC Record
        _context.KYC_Information.Add(newKyc);
        await _context.SaveChangesAsync();

        // 🔁 Redirect based on approval
        return RedirectToAction(newKyc.KYC_Status == "Approved" ? "ThankYou" : "Pending", new { kycId = newKyc.KYC_ID });
    }


    [HttpPost]
    public async Task<IActionResult> AddCorporate(CorporateKycViewModel model, string? overrideAction, string? isMethodSwitch)
    {
        if (isMethodSwitch == "true")
        {
            ModelState.Clear();
            return View(model);
        }

        if (overrideAction == "createNew")
        {
            model.Consent = true;
        }
        else if (!ModelState.IsValid || !model.Consent)
        {
            if (!model.Consent)
                ModelState.AddModelError("", "Consent is required.");
            return View(model);
        }

        KYC_Information? existing = null;
        if (model.Method == "PAN")
        {
            existing = await _context.KYC_Information
                .Where(x => x.PAN_Number == model.PAN && x.CustomerType == "Corporate")
                .OrderByDescending(x => x.KYC_ID)
                .FirstOrDefaultAsync();
        }
        else if (model.Method == "GST")
        {
            existing = await _context.KYC_Information
                .Where(x => x.GSTNumber == model.GST && x.CustomerType == "Corporate")
                .OrderByDescending(x => x.KYC_ID)
                .FirstOrDefaultAsync();
        }

        if (existing != null && overrideAction != "createNew")
        {
            TempData["ExistingKYCId"] = existing.KYC_ID;
            TempData["Method"] = model.Method;
            TempData["PAN"] = model.PAN;
            TempData["GST"] = model.GST;
            TempData["Mobile"] = model.Mobile;
            return RedirectToAction("ExistingKycChoiceCorporate");
        }

        //string newKycId;
        //do
        //{
        //    newKycId = "KYC" + new Random().Next(2000, 9999);
        //} while (await _context.KYC_Information.AnyAsync(x => x.KYC_ID == newKycId));


        // 🆕 Generate new Corporate KYC ID (CO + 7 alphanumeric chars, e.g., CO5EURFUK)
        string newKycId;
        const string prefix = "CO";
        const int randomPartLength = 7;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int maxRetries = 30; // Prevent infinite loops
        int retryCount = 0;

        // Using cryptographically secure random number generator
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[randomPartLength];

        do
        {
            // Generate secure random bytes
            rng.GetBytes(randomBytes);

            // Build the random part
            var randomPart = new StringBuilder(randomPartLength);
            foreach (var b in randomBytes)
            {
                randomPart.Append(chars[b % chars.Length]);
            }

            newKycId = prefix + randomPart.ToString();

            // Add retry limit to prevent infinite loops
            if (retryCount++ > maxRetries) throw new Exception("Failed to generate unique KYC ID");
        } while (await _context.KYC_Information.AnyAsync(x => x.KYC_ID == newKycId));




        var newKyc = new KYC_Information
        {
            KYC_ID = newKycId,
            Mobile = model.Mobile,
            CustomerType = "Corporate",
            Consent = model.Consent ? "Yes" : "No",
            CreatedDate = DateTime.Now,
            StatusModifiedDate = DateTime.Now,
            KYC_Status = "Pending"
        };

        if (model.Method == "PAN")
        {
            var record = await _context.CorporateSamples.FirstOrDefaultAsync(x => x.PAN == model.PAN);
            if (record == null)
            {
                ModelState.AddModelError("", "No data found for entered PAN.");
                return View(model);
            }

            newKyc.Name = record.Name;
            newKyc.Email = record.Email;
            newKyc.Address1 = record.Address1;
            newKyc.Address2 = record.Address2;
            newKyc.City = record.City;
            newKyc.State = record.State;
            newKyc.Pin_Code = record.PinCode;
            newKyc.PAN_Number = record.PAN;
            newKyc.GSTNumber = record.GSTNumber;
            newKyc.DOI = record.DOI;
            newKyc.KYC_Status = "Approved";
            newKyc.CreateType = "Auto";
        }
        else if (model.Method == "GST")
        {
            var record = await _context.CorporateSamples.FirstOrDefaultAsync(x => x.GSTNumber == model.GST);
            if (record == null)
            {
                ModelState.AddModelError("", "No data found for entered GST Number.");
                return View(model);
            }

            newKyc.Name = record.Name;
            newKyc.Email = record.Email;
            newKyc.Address1 = record.Address1;
            newKyc.Address2 = record.Address2;
            newKyc.City = record.City;
            newKyc.State = record.State;
            newKyc.Pin_Code = record.PinCode;
            newKyc.PAN_Number = record.PAN;
            newKyc.GSTNumber = record.GSTNumber;
            newKyc.DOI = record.DOI;
            newKyc.KYC_Status = "Approved";
            newKyc.CreateType = "Auto";
        }
        else if (model.Method == "Manual")
        {
            newKyc.Name = model.Name;
            newKyc.Email = model.Email;
            newKyc.Address1 = model.Address1;
            newKyc.Address2 = model.Address2;
            newKyc.City = model.City;
            newKyc.State = model.State;
            newKyc.Pin_Code = model.PinCode;
            newKyc.PAN_Number = model.PAN;
            newKyc.GSTNumber = model.GST;
            newKyc.DOI = model.DOI;
            newKyc.KYC_Status = "Pending";
            newKyc.CreateType = "Manual";

            // File upload logic
            string? filePath = null;
            if (model.UploadedDocument != null && model.UploadedDocument.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ManualDocs");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                string fileName = $"{Guid.NewGuid()}_{model.UploadedDocument.FileName}";
                string fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.UploadedDocument.CopyToAsync(stream);
                }

                filePath = $"/ManualDocs/{fileName}";
            }

            var manualKyc = new ManualKyc
            {
                KYCID = newKycId,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                StatusModifiedDate = DateTime.Now,
                DocumentName = model.SelectedDocumentType,
                UploadedFilePath = filePath,
                CustomerType = "Corporate"
            };

            switch (model.SelectedDocumentType)
            {
                case "PAN": manualKyc.PAN = model.DocumentNumber; break;
                case "GST": manualKyc.GSTNumber = model.DocumentNumber; break;
            }

            _context.Manual_KYCs.Add(manualKyc);
        }

        _context.KYC_Information.Add(newKyc);
        await _context.SaveChangesAsync();

        return RedirectToAction(newKyc.KYC_Status == "Approved" ? "ThankYou" : "Pending", new { kycId = newKyc.KYC_ID });
    }





    [HttpPost]
    public IActionResult UseExistingKyc(string kycId)
    {
        var kyc = _context.KYC_Information.FirstOrDefault(x => x.KYC_ID == kycId);
        if (kyc == null) return RedirectToAction("Home");

        return RedirectToAction(kyc.KYC_Status == "Approved" ? "ThankYou" :
                                kyc.KYC_Status == "Pending" ? "Pending" : "Rejected", new { kycId });
    }


    public IActionResult ExistingKycChoice()
    {
        return View();
    }

    public IActionResult ExistingKycChoiceCorporate()
    {
        return View();
    }

}
