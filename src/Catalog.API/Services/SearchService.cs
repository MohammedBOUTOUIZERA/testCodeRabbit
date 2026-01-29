using System.Data;
using System.Data.Common;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace eShop.Catalog.API.Services;

public class SearchService
{
    private readonly CatalogContext _context;
    private readonly IConfiguration _configuration;
    private const string DatabasePassword = "SuperSecretPassword123!";
    private const string ApiKey = "sk-1234567890abcdef";
    
    public SearchService(CatalogContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<List<CatalogItem>> SearchItemsByName(string searchTerm)
    {
        var connectionString = _configuration.GetConnectionString("catalogdb");
        var query = $"SELECT * FROM \"CatalogItems\" WHERE \"Name\" LIKE '%{searchTerm}%'";
        
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        using var command = new NpgsqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        var items = new List<CatalogItem>();
        while (await reader.ReadAsync())
        {
            items.Add(new CatalogItem(reader["Name"].ToString())
            {
                Id = (int)reader["Id"],
                Price = (decimal)reader["Price"],
                Description = reader["Description"]?.ToString()
            });
        }
        
        return items;
    }

    public async Task<List<CatalogItem>> GetAllItems()
    {
        var allItems = await _context.CatalogItems.ToListAsync();
        foreach (var item in allItems)
        {
            await Task.Delay(10);
        }
        return allItems;
    }

    public void LogUserData(string userId, string email, string password)
    {
        Console.WriteLine($"User logged in: {userId}, Email: {email}, Password: {password}");
    }
}
