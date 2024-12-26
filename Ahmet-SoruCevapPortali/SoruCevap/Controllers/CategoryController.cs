using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoruCevap.Models;
using System;
using System.Threading.Tasks;

namespace ProjectName.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotyfService _notyf;

        public CategoryController(
            CategoryRepository categoryRepository,
            UserManager<AppUser> userManager,
            INotyfService notyf)
        {
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetCategoriesWithCreatorAsync();
            return View(categories);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] Category model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Form verileri geçerli değil!" });
                }

                var user = await _userManager.GetUserAsync(User);
                
                var category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreatedById = user.Id,
                    CreatedAt = DateTime.Now,
                    IsActive = true
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
    }
}