using Microsoft.EntityFrameworkCore;
using SoruCevap.Models;
using SoruCevap.ViewModel;
using System.Threading.Tasks;

namespace SoruCevap.Repositories
{
    public class AnswerRepository : GenericRepository<Answer>
    {
        public AnswerRepository(AppDbContext context) : base(context, context.Answers)
        {
        }

        public async Task<Answer> GetAcceptedAnswer(int questionId)
        {
            return await _context.Answers
                .FirstOrDefaultAsync(a => a.QuestionId == questionId && a.IsAccepted);
        }

        public async Task<AnswerDetailViewModel> GetAnswerDetailsAsync(int id)
        {
            var answer = await _context.Answers
                .Include(a => a.CreatedBy)
                .Include(a => a.Question)
                    .ThenInclude(q => q.Category)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (answer == null)
                return null;

            return new AnswerDetailViewModel
            {
                Id = answer.Id,
                Content = answer.Content,
                CreatedAt = answer.CreatedAt,
                IsAccepted = answer.IsAccepted,
                CreatedBy = new UserViewModel
                {
                    Id = answer.CreatedBy.Id,
                    UserName = answer.CreatedBy.UserName,
                    Email = answer.CreatedBy.Email
                },
                Question = new QuestionViewModel
                {
                    Id = answer.Question.Id,
                    Title = answer.Question.Title,
                    Category = new KategoriViewModel
                    {
                        Id = answer.Question.Category.Id,
                        Name = answer.Question.Category.Name
                    }
                }
            };
        }
    }
}
