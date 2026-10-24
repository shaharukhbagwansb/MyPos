
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPos.API.Application.DTOs;
using MyPos.API.Domain.Entities;
using MyPos.API.Infrastructure.Data;

namespace MyPos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetClientId()
        {
            return int.Parse(
                User.FindFirst("ClientID")?.Value ?? "0"
            );
        }

        // GET ALL PRODUCTS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clientId = GetClientId();

            var list = await _context.Products
                .Where(p =>
                    p.ClientID == clientId &&
                    p.IsActive)
                .OrderBy(p => p.ProductName)
                .Select(p => new
                {
                    p.ProductId,

                    p.CategoryId,
                    CategoryName = p.Category.CategoryName,

                    p.BrandId,
                    BrandName = p.Brand != null
                        ? p.Brand.BrandName
                        : null,

                    p.UnitId,
                    UnitName = p.Unit.UnitName,

                    p.ProductCode,
                    p.Barcode,
                    p.ProductName,
                    p.ModelNo,
                    p.PurchaseRate,
                    p.SaleRate,
                    p.MinimumStock,
                    p.TaxPercent,
                    p.IsActive
                })
                .ToListAsync();

            return Ok(list);
        }

        // CREATE PRODUCT
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductRequest request)
        {
            var clientId = GetClientId();

            if (string.IsNullOrWhiteSpace(request.ProductName))
            {
                return BadRequest(new
                {
                    message = "Product Name is Required"
                });
            }

            if (request.PurchaseRate < 0 ||
                request.SaleRate < 0 ||
                request.MinimumStock < 0 ||
                request.TaxPercent < 0 ||
                request.TaxPercent > 100)
            {
                return BadRequest(new
                {
                    message = "Invalid product rates, stock or tax"
                });
            }

            var productName = request.ProductName.Trim();

            // VALIDATE CATEGORY
            var categoryExists = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryId == request.CategoryId &&
                    c.ClientID == clientId &&
                    c.IsActive);

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    message = "Invalid Category"
                });
            }

            // VALIDATE UNIT
            var unitExists = await _context.Units
                .AnyAsync(u =>
                    u.UnitId == request.UnitId &&
                    u.ClientID == clientId &&
                    u.IsActive);

            if (!unitExists)
            {
                return BadRequest(new
                {
                    message = "Invalid Unit"
                });
            }

            // VALIDATE OPTIONAL BRAND
            if (request.BrandId.HasValue)
            {
                var brandExists = await _context.Brands
                    .AnyAsync(b =>
                        b.BrandId == request.BrandId.Value &&
                        b.ClientID == clientId &&
                        b.IsActive);

                if (!brandExists)
                {
                    return BadRequest(new
                    {
                        message = "Invalid Brand"
                    });
                }
            }

            // CHECK DUPLICATE PRODUCT NAME
            var nameExists = await _context.Products
                .AnyAsync(p =>
                    p.ClientID == clientId &&
                    p.ProductName == productName &&
                    p.IsActive);

            if (nameExists)
            {
                return BadRequest(new
                {
                    message = "Product already exists"
                });
            }

            // GENERATE NEXT 8-DIGIT PRODUCT CODE
            // PRODUCT CODE AND BARCODE WILL BE SAME
            var existingCodes = await _context.Products
                .Where(p =>
                    p.ClientID == clientId &&
                    p.ProductCode != null)
                .Select(p => p.ProductCode!)
                .ToListAsync();

            int maxCode = existingCodes
                .Select(code =>
                    int.TryParse(code, out int number)
                        ? number
                        : 0)
                .DefaultIfEmpty(0)
                .Max();

            if (maxCode >= 99999999)
            {
                return BadRequest(new
                {
                    message = "8-digit product code limit reached"
                });
            }

            string productCode = (maxCode + 1).ToString("D8");

            // CREATE PRODUCT
            var product = new Product
            {
                ClientID = clientId,

                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                UnitId = request.UnitId,

                ProductCode = productCode,
                Barcode = productCode,

                ProductName = productName,

                ModelNo = string.IsNullOrWhiteSpace(request.ModelNo)
                    ? null
                    : request.ModelNo.Trim(),

                PurchaseRate = request.PurchaseRate,
                SaleRate = request.SaleRate,
                MinimumStock = request.MinimumStock,
                TaxPercent = request.TaxPercent,

                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                productId = product.ProductId,
                productCode = product.ProductCode,
                barcode = product.Barcode,
                productName = product.ProductName,
                message = "Product created successfully"
            });
        }

        // UPDATE PRODUCT
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateProductRequest request)
        {
            var clientId = GetClientId();

            if (string.IsNullOrWhiteSpace(request.ProductName))
            {
                return BadRequest(new
                {
                    message = "Product Name is Required"
                });
            }

            if (request.PurchaseRate < 0 ||
                request.SaleRate < 0 ||
                request.MinimumStock < 0 ||
                request.TaxPercent < 0 ||
                request.TaxPercent > 100)
            {
                return BadRequest(new
                {
                    message = "Invalid product rates, stock or tax"
                });
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.ClientID == clientId);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found"
                });
            }

            var productName = request.ProductName.Trim();

            // VALIDATE CATEGORY
            var categoryExists = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryId == request.CategoryId &&
                    c.ClientID == clientId &&
                    c.IsActive);

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    message = "Invalid Category"
                });
            }

            // VALIDATE UNIT
            var unitExists = await _context.Units
                .AnyAsync(u =>
                    u.UnitId == request.UnitId &&
                    u.ClientID == clientId &&
                    u.IsActive);

            if (!unitExists)
            {
                return BadRequest(new
                {
                    message = "Invalid Unit"
                });
            }

            // VALIDATE OPTIONAL BRAND
            if (request.BrandId.HasValue)
            {
                var brandExists = await _context.Brands
                    .AnyAsync(b =>
                        b.BrandId == request.BrandId.Value &&
                        b.ClientID == clientId &&
                        b.IsActive);

                if (!brandExists)
                {
                    return BadRequest(new
                    {
                        message = "Invalid Brand"
                    });
                }
            }

            // CHECK DUPLICATE PRODUCT NAME
            var nameExists = await _context.Products
                .AnyAsync(p =>
                    p.ClientID == clientId &&
                    p.ProductId != id &&
                    p.ProductName == productName &&
                    p.IsActive);

            if (nameExists)
            {
                return BadRequest(new
                {
                    message = "Product already exists"
                });
            }

            // UPDATE PRODUCT DETAILS
            // KEEP EXISTING PRODUCT CODE AND BARCODE
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;
            product.UnitId = request.UnitId;

            product.ProductName = productName;

            product.ModelNo = string.IsNullOrWhiteSpace(request.ModelNo)
                ? null
                : request.ModelNo.Trim();

            product.PurchaseRate = request.PurchaseRate;
            product.SaleRate = request.SaleRate;
            product.MinimumStock = request.MinimumStock;
            product.TaxPercent = request.TaxPercent;

            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                productId = product.ProductId,
                productCode = product.ProductCode,
                barcode = product.Barcode,
                message = "Product updated successfully"
            });
        }

        // DELETE PRODUCT (SOFT DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var clientId = GetClientId();

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.ClientID == clientId);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found"
                });
            }

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product deleted successfully"
            });
        }
    }
}
