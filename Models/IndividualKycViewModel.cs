using System;
using System.ComponentModel.DataAnnotations;

namespace KYCIDGenerator.Models
{
    public class IndividualKycViewModel
    {
        [Required]
        public string Method { get; set; } = "PAN"; // PAN | Aadhaar | Manual

        public string? PAN { get; set; }
        public string? Adhaar { get; set; }

        [Required]
        public string Mobile { get; set; }

        public bool Consent { get; set; }

        // For Manual entry
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? PinCode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public DateTime? DOB { get; set; }

        public string? Captcha { get; set; } // For now just a textbox, no validation logic

        public string? SelectedDocumentType { get; set; }

        [Display(Name = "Document Number")]
        public string? DocumentNumber { get; set; }

        public IFormFile? UploadedDocument { get; set; }

    }
}
