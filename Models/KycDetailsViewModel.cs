namespace KYCIDGenerator.Models
{
    public class KycDetailsViewModel
    {
        public KYC_Information Kyc { get; set; }
        public string? DocumentName { get; set; }
        public string? UploadedFilePath { get; set; }
        public bool IsApproved { get; set; }
    }

}
