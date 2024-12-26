namespace SoruCevap.Models
{
    public class QuestionVote
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public VoteType VoteType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public enum VoteType
    {
        UpVote,
        DownVote
    }
}
