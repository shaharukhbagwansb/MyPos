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



        //// SQL DATA TO BRAND ENTITY
        //private static Brand MapBrand(SqlDataReader reader)
        //{
        //    return new Brand
        //    {
        //        BrandId = reader.GetInt32(reader.GetOrdinal("BrandId")),
        //        ClientID = reader.GetInt32(reader.GetOrdinal("ClientID")),
        //        BrandName = reader.GetString(reader.GetOrdinal("BrandName")),
        //        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
        //        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        //    };
        //}
    }
}