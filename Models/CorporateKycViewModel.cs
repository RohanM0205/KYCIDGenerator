using System;
using System.ComponentModel.DataAnnotations;

namespace KYCIDGenerator.Models
{
    public class CorporateKycViewModel
    {
        public string Method { get; set; } = "PAN"; // Default to PAN

        // Common fields
        public string? PAN { get; set; }

        public string? GST { get; set; }

        [Required]
        public string Mobile { get; set; }

        [Required]
        public string Captcha { get; set; }

        public bool Consent { get; set; }

        // Manual input fields
        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Address1 { get; set; }

        public string? Address2 { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? PinCode { get; set; }

        public DateTime? DOI { get; set; } // Date of Incorporation

        public string? SelectedDocumentType { get; set; }

        [Display(Name = "Document Number")]
        public string? DocumentNumber { get; set; }

        public IFormFile? UploadedDocument { get; set; }
    }
}
