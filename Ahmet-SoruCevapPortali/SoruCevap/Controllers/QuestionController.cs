using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AspNetCoreHero.ToastNotification.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoruCevap.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using SoruCevap.Repositories;
using Microsoft.AspNetCore.SignalR;
using SoruCevap.ViewModel;

namespace YourNamespace.Controllers
{
    public class QuestionController : Controller
    {
        private readonly QuestionRepository _questionRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _env;
        private readonly AnswerRepository _answerRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public QuestionController(
            QuestionRepository questionRepository,
            CategoryRepository categoryRepository,
            UserManager<AppUser> userManager,
            INotyfService notyf,
            IWebHostEnvironment env,
            AnswerRepository answerRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _questionRepository = questionRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _notyf = notyf;
            _env = env;
            _answerRepository = answerRepository;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetCategoriesWithCreatorAsync();
            return View(categories);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateQuestion([FromBody] Question model)
        {
            try
            {
               

                var user = await _userManager.GetUserAsync(User);
                
                var question = new Question
                {
                    Title = model.Title,
                    Content = model.Content,
                    CategoryId = model.CategoryId,
                    CreatedById = user.Id,
                    CreatedAt = DateTime.Now,
                    Status = QuestionStatus.Pending,
                    Tags = model.Tags,
                    ViewCount = 0,
                    VoteCount = 0
                };

                await _questionRepository.AddAsync(question);

                return Json(new { 
                    success = true, 
                    message = "Sorunuz başarıyla oluşturuldu ve onay için gönderildi.",
                    questionId = question.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return Json(categories);
        }

        [Authorize]
        public async Task<IActionResult> MyQuestions()
        {
            var user = await _userManager.GetUserAsync(User);
            var questions = await _questionRepository.GetUserQuestionsAsync(user.Id);
            return View(questions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var question = await _questionRepository.GetQuestionWithDetailsAsync(id);
            
            if (question == null)
            {
                _notyf.Error("Soru bulunamadı!");
                return RedirectToAction("Index");
            }

            // Görüntülenme sayısını artır
            question.ViewCount++;
            await _questionRepository.UpdateAsync(question);

            return View(question);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddAnswer(int questionId, string content)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var question = await _questionRepository.GetQuestionWithDetailsAsync(questionId);
                
                if (question == null)
                {
                    return Json(new { success = false, message = "Soru bulunamadı." });
                }

                var answer = new Answer
                {
                    Content = content,
                    QuestionId = questionId,
                    CreatedById = user.Id,
                    CreatedAt = DateTime.Now,
                    IsAccepted = false
                };

                await _answerRepository.AddAsync(answer);

                // Önce SignalR ile yeni cevabı gönder
                await _hubContext.Clients.Group($"Question_{questionId}")
                    .SendAsync("ReceiveNewAnswer", content, user.UserName, user.Id);

                // Soru sahibine bildirim gönder (eğer cevabı yazan kişi soru sahibi değilse)
                if (question.CreatedById != user.Id)
                {
                    var notificationMessage = $"{user.UserName} sorunuza yeni bir cevap yazdı.";
                    await _hubContext.Clients.User(question.CreatedById)
                        .SendAsync("ReceiveAnswerNotification", notificationMessage, questionId);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { error = "Dosya seçilmedi." });

            try
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "answers");
                
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return Json(new { url = $"/uploads/answers/{fileName}" });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AcceptAnswer(int answerId)
        {
            try
            {
                var answer = await _answerRepository.GetByIdAsync(answerId);
                if (answer == null)
                {
                    return Json(new { success = false, message = "Cevap bulunamadı." });
                }

                var question = await _questionRepository.GetByIdAsync(answer.QuestionId);
                if (question == null)
                {
                    return Json(new { success = false, message = "Soru bulunamadı." });
                }

                // Sorunun sahibi olup olmadığını kontrol et
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (question.CreatedById != userId)
                {
                    return Json(new { success = false, message = "Bu işlem için yetkiniz yok." });
                }

                // Varsa önceden kabul edilmiş cevabı bul ve kabul edilme durumunu kaldır
                var previousAcceptedAnswer = await _answerRepository.GetAcceptedAnswer(question.Id);
                if (previousAcceptedAnswer != null)
                {
                    previousAcceptedAnswer.IsAccepted = false;
                    await _answerRepository.UpdateAsync(previousAcceptedAnswer);
                }

                // Yeni cevabı kabul et
                answer.IsAccepted = true;
                answer.AcceptedAt = DateTime.Now;
                await _answerRepository.UpdateAsync(answer);

                _notyf.Success("Cevap başarıyla kabul edildi!");
                return Json(new { success = true, message = "Cevap başarıyla kabul edildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        public async Task<IActionResult> Category(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _notyf.Error("Kategori bulunamadı!");
                return RedirectToAction("Index", "Home");
            }

            var questions = await _questionRepository.GetQuestionsByCategoryAsync(id);

            var viewModel = new CategoryViewModel
            {
                Category = category,
                Questions = questions
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(string type, int id)
        {
            if (type == "question")
            {
                var question = await _questionRepository.GetQuestionDetailsAsync(id);
                if (question == null) return NotFound();
                return Json(question);
            }
            else if (type == "answer")
            {
                var answer = await _answerRepository.GetAnswerDetailsAsync(id);
                if (answer == null) return NotFound();
                return Json(answer);
            }
            
            return BadRequest();
        }
    }
}