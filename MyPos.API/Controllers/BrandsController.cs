using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPos.API.Domain.Entities;
using MyPos.API.Services;
using MyPos.API.Application.DTOs;

namespace MyPos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BrandsController : ControllerBase
    {
        private readonly IMasterService _masterService;

        public BrandsController(IMasterService masterService)
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Brand request,[FromQuery] string logStr)
        {
            int clientId = GetClientId();
            int userId = GetUserId();

            if (clientId <= 0 || userId <= 0)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.BrandName))
                return BadRequest("Brand Name is required");

            if (string.IsNullOrWhiteSpace(logStr))
                return BadRequest("LogStr is required");

            var brand = new Brand
            {
                ClientID = clientId,
                BrandName = request.BrandName.Trim(),
                IsActive = true
            };

            int brandId = await _masterService
                .InsertBrandAsync(brand, userId, logStr);

            return Ok(new
            {
                brandId,
                brandName = brand.BrandName,
                isActive = brand.IsActive
            });
        }
        // GET: api/Brands
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int clientId = GetClientId();

            if (clientId <= 0)
            {
                return Unauthorized();
            }                

            var brands = await _masterService.GetBrandsAsync(clientId);

            return Ok(brands);
        }
        // UPDATE BRAND
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,[FromBody] Brand brand,[FromQuery] string logStr)
        {
            int clientId = GetClientId();
            int userId = GetUserId();

            if (clientId <= 0 || userId <= 0)
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Invalid BrandId");

            if (string.IsNullOrWhiteSpace(brand.BrandName))
                return BadRequest("Brand Name is required");

            if (string.IsNullOrWhiteSpace(logStr))
                return BadRequest("LogStr is required");

            brand.BrandId = id;
            brand.ClientID = clientId;
            brand.BrandName = brand.BrandName.Trim();
            brand.IsActive = brand.IsActive;

            bool updated = await _masterService.UpdateBrandAsync(brand, userId, logStr);

            if (!updated)
                return NotFound(new
                {
                    message = "Brand not found"
                });

            return Ok(new
            {
                message = "Brand updated successfully"
            });
        }
    }
}