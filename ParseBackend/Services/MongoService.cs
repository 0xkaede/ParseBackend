using MongoDB.Driver;
using ParseBackend.Models.Database;
using ParseBackend.Utils;

namespace ParseBackend.Services
{
    public interface IMongoService
    {

    }

    public class MongoService : IMongoService
    {
        private readonly IMongoCollection<UserData> _userProfiles;

        public MongoService()
        {
            var client = new MongoClient("mongodb://localhost");

            var mongoDatabase = client.GetDatabase("KaedeBackend");

            _userProfiles = mongoDatabase.GetCollection<UserData>("UserProfiles");

            _ = InitDatabase();
        }

        public async Task InitDatabase()
        {
            Logger.Log("Database Is Online");
        }


    }
}
