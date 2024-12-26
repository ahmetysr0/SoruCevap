using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using SoruCevap.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SoruCevap.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using SoruCevap.ViewModel;



namespace SoruCevap.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;
        private readonly QuestionRepository _questionRepository;

        public AdminController(
            CategoryRepository categoryRepository,
            UserManager<AppUser> userManager,
            INotyfService notyf,
            IWebHostEnvironment env,
            AppDbContext context,
            QuestionRepository questionRepository)
        {
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _notyf = notyf;
            _env = env;
            _context = context;
            _questionRepository = questionRepository;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Categories()
        {
            var categories = await _categoryRepository.GetCategoriesWithCreatorAsync();
            return View(categories);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromForm] CategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Form verileri geçerli değil!" });

                var user = await _userManager.GetUserAsync(User);
                
                // Resim işleme
                string imageUrl = null;
                if (model.ImageFile != null)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{model.ImageFile.FileName}";
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "categories");
                    
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                    
                    imageUrl = $"/uploads/categories/{uniqueFileName}";
                }

                var category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    ImageUrl = imageUrl,
                    IsActive = true,
                    CreatedById = user.Id,
                    CreatedAt = DateTime.Now,
                    QuestionCount = 0
                };

                await _categoryRepository.AddAsync(category);

                return Json(new { 
                    success = true, 
                    message = "Kategori başarıyla oluşturuldu.",
                    categoryId = category.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetParentCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return Json(categories.Select(c => new { c.Id, c.Name }));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Kategori başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingQuestions()
        {
            var pendingQuestions = await _context.Questions
                .Include(q => q.Category)
                .Include(q => q.CreatedBy)
                .Where(q => q.Status == QuestionStatus.Pending)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(pendingQuestions);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingQuestionsCount()
        {
            var count = await _context.Questions
                .Where(q => q.Status == QuestionStatus.Pending)
                .CountAsync();
            
            return Json(count);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewQuestion(int id, bool approved, string? rejectionReason)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
                return Json(new { success = false, message = "Soru bulunamadı." });

            question.Status = approved ? QuestionStatus.Approved : QuestionStatus.Rejected;
            question.ReviewedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
            question.ReviewedAt = DateTime.Now;
            question.RejectionReason = rejectionReason;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = approved ? "Soru onaylandı." : "Soru reddedildi." });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = new
            {
                TotalQuestions = await _context.Questions.CountAsync(),
                PendingQuestions = await _context.Questions.Where(q => q.Status == QuestionStatus.Pending).CountAsync(),
                ApprovedQuestions = await _context.Questions.Where(q => q.Status == QuestionStatus.Approved).CountAsync(),
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalAnswers = await _context.Answers.CountAsync(),
                SolvedQuestions = await _context.Questions
                    .Where(q => q.Answers.Any(a => a.IsAccepted))
                    .CountAsync(),
                LastWeekQuestions = await _context.Questions
                    .Where(q => q.CreatedAt >= DateTime.Now.AddDays(-7))
                    .CountAsync()
            };

            return Json(stats);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserList()
        {
            var users = await _userManager.Users
                .Select(u => new UserListViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                   
                    QuestionCount = _context.Questions.Count(q => q.CreatedById == u.Id),
                    AnswerCount = _context.Answers.Count(a => a.CreatedById == u.Id),
                    AcceptedAnswerCount = _context.Answers.Count(a => a.CreatedById == u.Id && a.IsAccepted),
                    
                })
                .ToListAsync();

            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Questions()
        {
            var questions = await _questionRepository.GetAllQuestionsWithDetailsAsync();
            return View(questions);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetQuestionDetails(int id)
        {
            var questionDetails = await _questionRepository.GetQuestionDetailsAsync(id);
            
            if (questionDetails == null)
                return NotFound();

            return Json(questionDetails);
        }
    }

    public class CategoryViewModel
    {
        [Required(ErrorMessage = "Kategori adı zorunludur")]
        public string Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
