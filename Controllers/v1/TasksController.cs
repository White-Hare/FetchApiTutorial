using System;
using FetchApiTutorial.Helpers;
using FetchApiTutorial.Models;
using FetchApiTutorial.Services.MyTaskService;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FetchApiTutorial.Helpers.Attributes;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FetchApiTutorial.Controllers.v1
{

    [Route("api/v1/[controller]")]
    [ApiController, Authorize]
    public class TasksController : ControllerBase
    {
        private readonly IMyTaskService _myTaskService;

        public TasksController(IMyTaskService myTaskService)
        {
            _myTaskService = myTaskService;
        }

        // GET: api/<TasksController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MyTask>>> Get()
        {
            var list = await _myTaskService.GetAllAsync();

            if (list.Any())
                return Ok(list);

            return NoContent();
        }

        // GET api/<TasksController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MyTask>> Get(string id)
        {
            MyTask task = await _myTaskService.GetAsync(id);

            if (task == null)
                return NotFound();

            return Ok(task);
        }

        // POST api/<TasksController>
        [HttpPost]
        public async Task<ActionResult<MyTask>> Post([FromBody] MyTask task)
        {
            if (ModelState.IsValid && !task.Equals(new MyTask()))
            {
                task.Id = ObjectId.GenerateNewId();
                await _myTaskService.AddAsync(task);
            }

            return CreatedAtAction("Get", new { id = task.Id.ToString() }, task);
        }

        // PUT api/<TasksController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<MyTask>> Put(string id, [FromBody] MyTask task)
        {
            if (ModelState.IsValid)
            {
                task.Id = new ObjectId(id);
                bool success = await _myTaskService.UpdateAsync(id, task);
                if (success)
                    return CreatedAtAction("Get", new { id = id}, task);
            }

            return NotFound();
        }

        // DELETE api/<TasksController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            bool success = await _myTaskService.DeleteAsync(id);
            if (success)
                return NoContent();

            return NotFound();
        }
    }
}
