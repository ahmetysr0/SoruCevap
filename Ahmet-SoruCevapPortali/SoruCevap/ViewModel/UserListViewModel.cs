namespace SoruCevap.ViewModel
{
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public int QuestionCount { get; set; }
        public int AnswerCount { get; set; }
        public int AcceptedAnswerCount { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
