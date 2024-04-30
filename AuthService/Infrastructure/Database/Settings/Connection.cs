using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AuthService.Infrastructure.Database.Settings;

public class Connection
{
    public MongoClient Client { get => _mongoClient;  }
    private MongoClient _mongoClient;

    public Connection(
        IConfiguration appSettings)
    {
        _mongoClient = new MongoClient(
            appSettings.GetValue<string>("AuthenticationDatabase:ConnectionString"));
    }
}
