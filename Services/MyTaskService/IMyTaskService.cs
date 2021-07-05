using FetchApiTutorial.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FetchApiTutorial.Services.MyTaskService
{
    public interface IMyTaskService
    {
        public Task<List<MyTask>> GetAllAsync();
        public Task<MyTask> GetAsync(string title);
        public Task AddAsync(MyTask task);
        public Task<bool> UpdateAsync(string title, MyTask task);
        public Task<bool> DeleteAsync(string title);

    }
}
