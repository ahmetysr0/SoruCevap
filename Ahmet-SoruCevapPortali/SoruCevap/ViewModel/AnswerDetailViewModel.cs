using SoruCevap.ViewModel;

public class AnswerDetailViewModel
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAccepted { get; set; }
    public UserViewModel CreatedBy { get; set; }
    public QuestionViewModel Question { get; set; }
}



