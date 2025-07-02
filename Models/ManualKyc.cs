namespace KYCIDGenerator.Models
{
    public class ManualKyc
    {
        public int Id { get; set; }

        public string KYCID { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? StatusModifiedDate { get; set; }

        public string? DocumentName { get; set; }

        public string? PAN { get; set; }

        public string? ADHAAR { get; set; }

        public string? LICENCE { get; set; }

        public string? PASSPORT { get; set; }

        public string? GSTNumber { get; set; }

        public string? UploadedFilePath { get; set; }

        public string? Remarks { get; set; }

        public string? CustomerType { get; set; }
    }

}
