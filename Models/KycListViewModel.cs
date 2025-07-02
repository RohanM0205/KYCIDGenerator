namespace KYCIDGenerator.Models
{
    public class KycListViewModel
    {
        public List<KYC_Information> Records { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? CustomerTypeFilter { get; set; }
        public string? CreateTypeFilter { get; set; }
        public DateTime? CreatedDateFilter { get; set; }
        public DateTime? StatusModifiedDateFilter { get; set; }
        public string? SearchQuery { get; set; }

        public string? RejectedReason { get; set; }
        public string? StatusFilter { get; set; } // ← New for Status dropdown


    }

}
