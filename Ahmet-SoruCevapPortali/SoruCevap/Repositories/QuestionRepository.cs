using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using SoruCevap.Models;
using SoruCevap.Repositories;
using SoruCevap.ViewModel;

public class QuestionRepository : GenericRepository<Question>
{
    public QuestionRepository(AppDbContext context) : base(context, context.Questions)
    {
    }

    public async Task<List<Question>> GetQuestionsWithDetailsAsync()
    {
        return await _context.Questions
            .Include(q => q.Category)
            .Include(q => q.CreatedBy)
            .Include(q => q.Answers)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<Question> GetQuestionWithDetailsAsync(int id)
    {
        return await _context.Questions
            .Include(q => q.Category)
            .Include(q => q.CreatedBy)
            .Include(q => q.Answers)
                .ThenInclude(a => a.CreatedBy)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<List<Question>> GetUserQuestionsAsync(string userId)
    {
        return await _context.Questions
            .Include(q => q.Category)
            .Include(q => q.CreatedBy)
            .Where(q => q.CreatedById == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Question>> GetApprovedQuestionsAsync()
    {
        return await _context.Questions
            .Include(q => q.Category)
            .Include(q => q.CreatedBy)
            .Include(q => q.Answers)
            .Where(q => q.Status == QuestionStatus.Approved)
            .OrderByDescending(q => q.CreatedAt)
             // Son 10 soru
            .ToListAsync();
    }

    public async Task<List<Question>> GetQuestionsByCategoryAsync(int categoryId)
    {
        return await _context.Questions
            .Include(q => q.CreatedBy)
            .Include(q => q.Answers)
            .Where(q => q.CategoryId == categoryId && q.Status == QuestionStatus.Approved)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Question>> GetAllQuestionsWithDetailsAsync()
    {
        return await _context.Questions
            .Include(q => q.Category)
            .Include(q => q.CreatedBy)
            .Include(q => q.Answers)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<QuestionDetailViewModel> GetQuestionDetailsAsync(int id)
    {
        var question = await _context.Questions
            .Include(q => q.Category)
            .Include(q => q.CreatedBy)
            .Include(q => q.Answers)
                .ThenInclude(a => a.CreatedBy)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return null;

        return new QuestionDetailViewModel
        {
            Id = question.Id,
            Title = question.Title,
            Content = question.Content,
            CreatedAt = question.CreatedAt,
            ViewCount = question.ViewCount,
            Category = new KategoriViewModel
            {
                Id = question.Category.Id,
                Name = question.Category.Name
            },
            CreatedBy = new UserViewModel
            {
                Id = question.CreatedBy.Id,
                UserName = question.CreatedBy.UserName,
                Email = question.CreatedBy.Email
            },
            Answers = question.Answers?.Select(a => new AnswerViewModel
            {
                Id = a.Id,
                Content = a.Content,
                CreatedAt = a.CreatedAt,
                IsAccepted = a.IsAccepted,
                CreatedBy = new UserViewModel
                {
                    Id = a.CreatedBy.Id,
                    UserName = a.CreatedBy.UserName,
                    Email = a.CreatedBy.Email
                }
            }).ToList(),
            Tags = question.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        };
    }
}






