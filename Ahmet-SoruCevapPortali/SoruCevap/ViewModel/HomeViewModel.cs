using SoruCevap.Models;

namespace SoruCevap.ViewModel
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; }
        public List<Question> ApprovedQuestions { get; set; }
    }
}
