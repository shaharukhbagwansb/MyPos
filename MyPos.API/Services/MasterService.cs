using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using MyPos.API.Domain.Entities;
using MyPos.API.Infrastructure.Data;
using System.Data;

namespace MyPos.API.Services
{
    public class MasterService : IMasterService
    {
        private readonly DbConnectionFactory _db;

        public MasterService(DbConnectionFactory db)
        {
            _db = db;
        }

        // GET ALL BRANDS
        public async Task<List<Brand>> GetBrandsAsync(int clientId)
        {
            var brands = new List<Brand>();

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand("spGetAllBrands", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

            await con.OpenAsync();

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                //brands.Add(MapBrand(reader));
                Brand objBr = new Brand();

                objBr.BrandId = reader.GetInt32(reader.GetOrdinal("BrandId"));
                objBr.ClientID = reader.GetInt32(reader.GetOrdinal("ClientID"));
                objBr.BrandName = reader.GetString(reader.GetOrdinal("BrandName"));
                objBr.IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
                objBr.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));

                brands.Add(objBr);
            }

            return brands;
        }

        // INSERT BRAND
        public async Task<int> InsertBrandAsync(Brand brand, int userId, string logStr)
        {
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand("dbo.spInsertBrands", con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = brand.ClientID;

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            cmd.Parameters.Add("@BrandName", SqlDbType.NVarChar, 200).Value = brand.BrandName;

            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = brand.IsActive;

            cmd.Parameters.Add("@LogStr", SqlDbType.NVarChar, -1).Value = logStr;

            await con.OpenAsync();

            object? result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        // UPDATE BRAND
        public async Task<bool> UpdateBrandAsync(Brand brand, int userId, string logStr)
        {
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand("spUpdateBrands", con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@BrandId", SqlDbType.Int).Value = brand.BrandId;

            cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = brand.ClientID;

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            cmd.Parameters.Add("@BrandName", SqlDbType.NVarChar, 200).Value = brand.BrandName;

            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = brand.IsActive;

            cmd.Parameters.Add("@LogStr", SqlDbType.NVarChar, -1).Value = logStr;

            await con.OpenAsync();

            object? result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        // GET ALL CATEGORIES
        public async Task<List<Category>> GetCategoriesAsync(int clientId)
        {
            var categories = new List<Category>();

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand("dbo.spGetAllCategories", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Category category = new Category();

                category.CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"));
                category.ClientID = reader.GetInt32(reader.GetOrdinal("ClientID"));
                category.CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"));

                int descriptionIndex = reader.GetOrdinal("Description");
                category.Description = reader.IsDBNull(descriptionIndex)
                    ? null
                    : reader.GetString(descriptionIndex);

                category.IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
                category.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));

                categories.Add(category);
            }

            return categories;
        }

        // INSERT CATEGORY
        public async Task<int> InsertCategoryAsync(Category category, int userId, string logStr)
        {
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand("dbo.spInsertCategories", con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = category.ClientID;
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@CategoryName", SqlDbType.NVarChar, 100).Value = category.CategoryName;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value =
                (object?)category.Description ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = category.IsActive;
            cmd.Parameters.Add("@LogStr", SqlDbType.NVarChar, -1).Value = logStr;

            await con.OpenAsync();

            object? result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        // UPDATE CATEGORY
        public async Task<bool> UpdateCategoryAsync(Category category, int userId, string logStr)
        {
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand("dbo.spUpdateCategories", con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value = category.CategoryId;
            cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = category.ClientID;
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@CategoryName", SqlDbType.NVarChar, 100).Value = category.CategoryName;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value =
                (object?)category.Description ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = category.IsActive;
            cmd.Parameters.Add("@LogStr", SqlDbType.NVarChar, -1).Value = logStr;

            await con.OpenAsync();

            object? result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        // GET ALL UNITS
        public async Task<List<Unit>> GetUnitsAsync(int clientId)
        {
            var units = new List<Unit>();

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand("dbo.spGetAllUnits", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Unit unit = new Unit();

                unit.UnitId = reader.GetInt32(reader.GetOrdinal("UnitId"));
                unit.ClientID = reader.GetInt32(reader.GetOrdinal("ClientID"));
                unit.UnitName = reader.GetString(reader.GetOrdinal("UnitName"));
                unit.ShortName = reader.GetString(reader.GetOrdinal("ShortName"));
                unit.IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));

                units.Add(unit);
            }

            return units;
        }

        // INSERT UNIT
        public async Task<int> InsertUnitAsync(Unit unit, int userId, string logStr)
        {
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand("dbo.spInsertUnits", con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = unit.ClientID;
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@UnitName", SqlDbType.NVarChar, 30).Value = unit.UnitName;
            cmd.Parameters.Add("@ShortName", SqlDbType.NVarChar, 10).Value = unit.ShortName;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = unit.IsActive;
            cmd.Parameters.Add("@LogStr", SqlDbType.NVarChar, -1).Value = logStr;

            await con.OpenAsync();

            object? result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        // UPDATE UNIT
        public async Task<bool> UpdateUnitAsync(Unit unit, int userId, string logStr)
        {
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand("dbo.spUpdateUnits", con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@UnitId", SqlDbType.Int).Value = unit.UnitId;
            cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = unit.ClientID;
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@UnitName", SqlDbType.NVarChar, 30).Value = unit.UnitName;
            cmd.Parameters.Add("@ShortName", SqlDbType.NVarChar, 10).Value = unit.ShortName;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = unit.IsActive;
            cmd.Parameters.Add("@LogStr", SqlDbType.NVarChar, -1).Value = logStr;

            await con.OpenAsync();

            object? result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

    }
}