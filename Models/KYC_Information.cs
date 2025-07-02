using System;
using System.ComponentModel.DataAnnotations;

namespace KYCIDGenerator.Models
{
    public class KYC_Information
    {
        [Key]
        public string KYC_ID { get; set; }

        public string Name { get; set; }
        public string Mobile { get; set; }
        public string? Email { get; set; }
        public string? PAN_Number { get; set; }
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        public string Pin_Code { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string KYC_Status { get; set; }
        public DateTime? DOB { get; set; }
        public string? CustomerType { get; set; }
        public string? AdhaarNumber { get; set; }
        public string? GSTNumber { get; set; }
        public DateTime? DOI { get; set; }

        public string? Consent { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? StatusModifiedDate { get; set; }

        public string? CreateType { get; set; }
    }
}
