namespace SoruCevap.Models
{
    public class QuestionReport
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public string ReportedById { get; set; }
        public AppUser ReportedBy { get; set; }
        public string Reason { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReviewedById { get; set; }
        public AppUser? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
    public enum ReportStatus
    {
        Pending,
        Reviewed,
        Ignored
    }
}
