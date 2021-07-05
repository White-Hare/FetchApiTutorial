using FetchApiTutorial.Helpers.Settings;
using FetchApiTutorial.Models;
using FetchApiTutorial.Models.User;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Authentication;

namespace FetchApiTutorial.Data
{
    public class MongoDatabase : IDatabase
    {
        public readonly IMongoCollection<MyTask> Posts;
        public readonly IMongoCollection<MyUser> Users;


        public MongoDatabase(IOptions<DatabaseSettings> options)
        {
            DatabaseSettings databaseSettings = options.Value;

            MongoClientSettings settings = MongoClientSettings.FromUrl(
                new MongoUrl(databaseSettings.ConnectionString)
            );
            settings.SslSettings =
                new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);
            var database = mongoClient.GetDatabase(databaseSettings.DatabaseName);


            Posts = database.GetCollection<MyTask>("Tasks");
            Users = database.GetCollection<MyUser>("Users");
        }
    }
}
