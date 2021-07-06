using System;
using FetchApiTutorial.Helpers;
using FetchApiTutorial.Models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FetchApiTutorial.Helpers.Attributes;

namespace FetchApiTutorial.Services.MyTaskService
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

    [Authorize]
    public class MyTaskService : IMyTaskService
    {
        private readonly List<MyTask> _tasks;


        public MyTaskService()
        {
            _tasks = new List<MyTask>();
            _tasks.Add(new MyTask { Id = ObjectId.GenerateNewId(), Title = "deneme", Content = "icerik" });
        }

        public async Task AddAsync(MyTask task)
        {
            _tasks.Add(task);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            MyTask rt = _tasks.Find(t => t.Id.ToString() == id);

            if (rt != null)
            {
                _tasks.Remove(rt);
                return true;
            }

            return false;
        }

        public async Task<MyTask> GetAsync(string id)
        {
            return _tasks.Find(t => t.Id.ToString() == id);
        }

        public async Task<List<MyTask>> GetAllAsync()
        {
            return _tasks.ToList();
        }

        public async Task<bool> UpdateAsync(string id, MyTask task)
        {
            MyTask rt = _tasks.Find(t => t.Id == new ObjectId(id));
            int index = _tasks.IndexOf(rt);

            Console.WriteLine(rt.Id);

            if (rt != null)
            {
                _tasks.Remove(rt);
                _tasks.Insert(index, task);

                return true;
            }

            return false;
        }
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

}
