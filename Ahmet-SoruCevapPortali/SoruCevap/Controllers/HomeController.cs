using SoruCevap.Models;
using SoruCevap.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using SoruCevap.ViewModel;


namespace SoruCevap.Controllers
{
    public class HomeController : Controller
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly QuestionRepository _questionRepository;

        public HomeController(
            CategoryRepository categoryRepository,
            QuestionRepository questionRepository)
        {
            _categoryRepository = categoryRepository;
            _questionRepository = questionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                Categories = await _categoryRepository.GetCategoriesWithQuestionCountAsync(),
                ApprovedQuestions = await _questionRepository.GetApprovedQuestionsAsync()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
