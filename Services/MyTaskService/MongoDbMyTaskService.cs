using System;
using FetchApiTutorial.Data;
using FetchApiTutorial.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FetchApiTutorial.Services.MyTaskService
{
    public class MongoDbMyTaskService : IMyTaskService
    {
        private readonly MongoDatabase _context;

        public MongoDbMyTaskService(IDatabase context)
        {
            _context = (MongoDatabase)context;
        }

        public async Task AddAsync(MyTask task)
        {
            await _context.Posts.InsertOneAsync(task);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            MyTask rt = await _context.Posts.FindOneAndDeleteAsync(Builders<MyTask>.Filter.Eq(t => t.Id, new ObjectId(id)));
            return rt != null;
        }

        public async Task<MyTask> GetAsync(string id)
        {
            var filter = Builders<MyTask>.Filter.Eq(t => t.Id, new ObjectId(id));
            return await _context.Posts.Find(filter).SingleOrDefaultAsync();
        }

        public async Task<List<MyTask>> GetAllAsync()
        {
            return await _context.Posts.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<bool> UpdateAsync(string id, MyTask task)
        {
            var filter = Builders<MyTask>.Filter.Eq(t => t.Id, new ObjectId(id));
            return await _context.Posts.ReplaceOneAsync(filter, task) != null;
        }

    }
}
