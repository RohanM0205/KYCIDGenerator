using System;

namespace KYCIDGenerator.Models
{
    public class CorporateSample
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string? Email { get; set; }
        public string PAN { get; set; }
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        public string PinCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string GSTNumber { get; set; }
        public DateTime DOI { get; set; }
    }
}
