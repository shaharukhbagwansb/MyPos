using MyPos.API.Domain.Entities;

namespace MyPos.API.Services
{
    public interface IMasterService
    {
        // BRANDS
        Task<List<Brand>> GetBrandsAsync(int clientId);
        Task<int> InsertBrandAsync(Brand brand, int userId, string logStr);
        Task<bool> UpdateBrandAsync(Brand brand, int userId, string logStr);
        // CATEGORIES
        Task<List<Category>> GetCategoriesAsync(int clientId);
        Task<int> InsertCategoryAsync(Category category, int userId, string logStr);
        Task<bool> UpdateCategoryAsync(Category category, int userId, string logStr);

        // UNITS
        Task<List<Unit>> GetUnitsAsync(int clientId);
        Task<int> InsertUnitAsync(Unit unit, int userId, string logStr);
        Task<bool> UpdateUnitAsync(Unit unit, int userId, string logStr);
    }
}