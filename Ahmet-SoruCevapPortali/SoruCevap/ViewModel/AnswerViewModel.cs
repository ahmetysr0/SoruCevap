namespace SoruCevap.ViewModel
{
    public class AnswerViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAccepted { get; set; }
        public UserViewModel CreatedBy { get; set; }
    }
}
