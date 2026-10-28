using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPos.API.Domain.Entities;
using MyPos.API.Services;

namespace MyPos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly IMasterService _masterService;

        public CategoriesController(IMasterService masterService)
        {
            _masterService = masterService;
        }

        private int GetClientId()
        {
            var value = User.FindFirst("ClientID")?.Value;
            return int.TryParse(value, out int id) ? id : 0;
        }

        private int GetUserId()
        {
            var value = User.FindFirst("UserId")?.Value;
            return int.TryParse(value, out int id) ? id : 0;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            int clientId = GetClientId();

            if (clientId <= 0)
                return Unauthorized();

            var categories = await _masterService.GetCategoriesAsync(clientId);

            return Ok(categories);
        }

        // INSERT CATEGORY
        [HttpPost]
        public async Task<IActionResult> CreateCategories([FromBody] Category request,[FromQuery] string logStr)
        {
            int clientId = GetClientId();
            int userId = GetUserId();

            if (clientId <= 0 || userId <= 0)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.CategoryName))
                return BadRequest("Category Name is required");

            if (request.CategoryName.Trim().Length > 100)
                return BadRequest("Category Name cannot exceed 100 characters");

            if (request.Description?.Length > 250)
                return BadRequest("Description cannot exceed 250 characters");

            if (string.IsNullOrWhiteSpace(logStr))
                return BadRequest("LogStr is required");

            var category = new Category
            {
                ClientID = clientId,
                CategoryName = request.CategoryName.Trim(),
                Description = request.Description,
                IsActive = true
            };

            int categoryId = await _masterService.InsertCategoryAsync(category, userId, logStr);

            return Ok(new
            {
                categoryId,
                categoryName = category.CategoryName,
                description = category.Description,
                isActive = category.IsActive
            });
        }

        // UPDATE CATEGORY
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategories(int id,[FromBody] Category category,[FromQuery] string logStr)
        {
            int clientId = GetClientId();
            int userId = GetUserId();

            if (clientId <= 0 || userId <= 0)
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Invalid CategoryId");

            if (string.IsNullOrWhiteSpace(category.CategoryName))
                return BadRequest("Category Name is required");

            if (category.CategoryName.Trim().Length > 100)
                return BadRequest("Category Name cannot exceed 100 characters");

            if (category.Description?.Length > 250)
                return BadRequest("Description cannot exceed 250 characters");

            if (string.IsNullOrWhiteSpace(logStr))
                return BadRequest("LogStr is required");

            category.CategoryId = id;
            category.ClientID = clientId;
            category.CategoryName = category.CategoryName.Trim();

            bool updated = await _masterService.UpdateCategoryAsync(category, userId, logStr);

            if (!updated)
                return NotFound(new { message = "Category not found" });

            return Ok(new { message = "Category updated successfully" });
        }
    }
}