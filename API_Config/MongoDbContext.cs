using Gestion_des_entreprises_dans_la_registre_de_commerce_de_Suisse;
using MongoDB.Driver;

public class MongoDBContext
{
    private readonly IMongoDatabase _database;

    public MongoDBContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDBConnection");
        var databaseName = "zefix_company"; // Replace with your actual database name

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<CompanyInfo> Companies => _database.GetCollection<CompanyInfo>("ListCompany");
}
