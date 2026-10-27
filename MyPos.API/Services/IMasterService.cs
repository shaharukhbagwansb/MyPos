using MyPos.API.Domain.Entities;

namespace MyPos.API.Services
{
    public interface IMasterService
    {
        // BRANDS
        Task<List<Brand>> GetBrandsAsync(int clientId);
        Task<int> InsertBrandAsync(Brand brand, int userId, string logStr);
        Task<bool> UpdateBrandAsync(Brand brand, int userId, string logStr);
        // Future: Units, Categories, Products
    }
}