using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using SoruCevap.Models;
using SoruCevap.Repositories;

public class CategoryRepository : GenericRepository<Category>
{
    public CategoryRepository(AppDbContext context) : base(context, context.Categories)
    {
    }

    public async Task<List<Category>> GetCategoriesWithCreatorAsync()
    {
        return await _context.Categories
            .Include(c => c.CreatedBy)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Category> GetCategoryWithQuestionsAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Questions)
            .Include(c => c.CreatedBy)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Category>> GetCategoriesWithQuestionCountAsync()
    {
        return await _context.Categories
            .Include(c => c.Questions)
                .ThenInclude(q => q.Answers)
            .Include(c => c.CreatedBy)
            .OrderBy(c => c.Name)
            .Select(c => new Category
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                CreatedById = c.CreatedById,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                Questions = c.Questions.Where(q => q.Status == QuestionStatus.Approved).ToList()
            })
            .ToListAsync();
    }
} 