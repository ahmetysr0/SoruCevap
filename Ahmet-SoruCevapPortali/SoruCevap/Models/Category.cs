namespace SoruCevap.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int QuestionCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedById { get; set; }
        public AppUser CreatedBy { get; set; }
        public string? UpdatedById { get; set; }
        public AppUser? UpdatedBy { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
