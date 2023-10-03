using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IDistributedCache _cache;
        public TaskController(ApplicationDBContext context, IDistributedCache cache) {
            _context = context;
            _cache = cache;
        }

        //GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<List<TaskModel>>> GetTasks() {
            var cachedTasks = await _cache.GetStringAsync("TaskCache");
            
            if (cachedTasks != null) { 
                var tasks = JsonConvert.DeserializeObject<List<TaskModel>>(cachedTasks);
                return Ok(tasks);
            }

            var tasksFromDb = await _context.Tasks.ToListAsync();

            var serializedTasks = JsonConvert.SerializeObject(tasksFromDb);

            await _cache.SetStringAsync("TasksCache", serializedTasks, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            });

            return Ok(tasksFromDb);
        }

        //GET: api/Task/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskModel>> GetTask(int id) { 
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) { 
                return NotFound();
            }

            return task;
        }

        //POST:  api/Task
        [HttpPost]
        public async Task<ActionResult<TaskModel>> SaveTask(TaskModel task) { 
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        //PUT: api/Task/5
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTask(int id, TaskModel task) {
            if (id != task.Id)
            {
                return BadRequest();
            }

            _context.Entry(task).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!_context.Tasks.Any(x => x.Id == id))
                {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }

        //DELETE api/Task/5
        [HttpDelete]
        public async Task<ActionResult> DeleteTask(int id) {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
