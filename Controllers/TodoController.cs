using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoItemsController(TodoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            var nameId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }


            if (!int.TryParse(nameId, out var userId))
            {
                return Unauthorized("Invalid or missing NameIdentifier claim.");
            }

            return await _context.TodoItems
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        [HttpGet("test-token")]
        public IActionResult TestToken([FromHeader(Name = "Authorization")] string authHeader)
        {
            Console.WriteLine($"Received auth header: {authHeader}");
            return Ok(new { header = authHeader });
        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todoItem == null)
                return NotFound();
            return todoItem;
        }

        // POST: api/TodoItems
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            todoItem.UserId = userId;
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        // PUT: api/TodoItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(int id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
                return BadRequest();

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var existingItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (existingItem == null)
                return NotFound();

            existingItem.Name = todoItem.Name;
            existingItem.IsComplete = todoItem.IsComplete;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TodoItems.Any(e => e.Id == id && e.UserId == userId))
                    return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE: api/TodoItems/5
        [Authorize(Roles = "Admin")] // Restrict to Admin role
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todoItem == null)
                return NotFound();

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}