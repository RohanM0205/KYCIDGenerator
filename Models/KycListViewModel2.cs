namespace KYCIDGenerator.Models
{
    public class KycListViewModel2
    {
        // List of formatted display-ready records
        public List<KycViewModel> Records { get; set; } = new();

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Filters
        public string? CustomerTypeFilter { get; set; }
        public string? CreateTypeFilter { get; set; }
        public DateTime? CreatedDateFilter { get; set; }
        public DateTime? StatusModifiedDateFilter { get; set; }
        public string? SearchQuery { get; set; }

        // For consistency if needed at top level (optional use)
        public string? RejectedReason { get; set; }
    }

    public class KycViewModel
    {
        public string KYC_ID { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? CustomerType { get; set; }
        public string? CreateType { get; set; }
        public string? KYC_Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? StatusModifiedDate { get; set; }

        public string? RejectedReason { get; set; } = "N/A";
    }
}
