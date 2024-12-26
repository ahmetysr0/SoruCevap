namespace SoruCevap.ViewModel
{
    public class QuestionDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
        public KategoriViewModel Category { get; set; }
        public UserViewModel CreatedBy { get; set; }
        public List<AnswerViewModel> Answers { get; set; }
        public List<string> Tags { get; set; }
    }
}
