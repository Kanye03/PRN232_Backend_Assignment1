using MongoDB.Driver;
using PRN232_Assignment1.Models;

namespace PRN232_Assignment1.Data;

public class ProductContext
{
    private readonly IMongoDatabase _database;

    public ProductContext(IMongoClient mongoClient, string databaseName)
    {
        _database = mongoClient.GetDatabase(databaseName);
    }

    public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
}