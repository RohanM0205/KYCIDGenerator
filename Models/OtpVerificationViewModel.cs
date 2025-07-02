namespace KYCIDGenerator.Models
{
    public class OtpVerificationViewModel
    {
        public string Mobile { get; set; }
        public string Otp { get; set; }
        public int? RemainingTime { get; set; }
    }
}
