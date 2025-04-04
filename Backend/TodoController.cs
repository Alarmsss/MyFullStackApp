using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFullStackApp.Backend.Models;

[Authorize]
[Route("todos")]
[ApiController]
public class TodoController : ControllerBase
{
    private readonly TodoDbContext _context;
    private const int MaxPageSize = 100;

    public TodoController(TodoDbContext context) //傳述TodoDbContext class!!!
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TodoItem>>> GetTodos(
        //[FromQuery] 會將參數從 URL 中取得
        [FromQuery] bool? completed, 
        [FromQuery] string? title,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "id",
        [FromQuery] string? order = "asc")
    {
        if (page < 1)
        {
            return BadRequest("Page must be greater than 0");
        }
        pageSize = Math.Min(pageSize, MaxPageSize);
        if (pageSize < 1)
        {
            return BadRequest("Page size must be between 1 and 100");
        }

        //sortBy = string.IsNullOrWhiteSpace(sortBy) ? "id" : sortBy.Trim().ToLower();
        //order = string.IsNullOrWhiteSpace(order) ? "asc" : order.Trim().ToLower();

        sortBy = sortBy?.Trim().ToLower();
        order = order?.Trim().ToLower();
        if (string.IsNullOrEmpty(sortBy))
        {
            sortBy = "id";
        }
        if (string.IsNullOrEmpty(order))
        {
            order = "asc";
        }

        if (!TodoItem.validSortFields.Contains(sortBy))
        {
            return BadRequest("SortBy must be 'id' or 'title'");
        }
        if (order != "asc" && order != "desc")
        {
            return BadRequest("Order must be 'asc' or 'desc'");
        }

        var query = _context.Todos.AsQueryable(); //建立查詢(還沒開始查詢)

        query = sortBy switch
        {
            "id" => order == "asc" ? query.OrderBy(t => t.Id) : query.OrderByDescending(t => t.Id),
            "title" => order == "asc" ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title)
        };

        //篩選是否完成
        if (completed.HasValue)
        {
            query = query.Where(t => t.IsCompleted == completed.Value);
        }
        //篩選標題
        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(t => t.Title.Contains(title));
        }
        //Skip 跳過前面幾筆資料，Take 取得幾筆資料
        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        

        //EF Core 會將所有 Where() 和 Skip()/Take() 轉換成 SQL，然後執行查詢
        return await query.ToListAsync();
    }

    //在POST方法中使用
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodoById(int id)
    {
        var todo = await _context.Todos.FindAsync(id);
        if (todo == null)
        {
            return NotFound();
        }
        return Ok(todo);
    }

    [HttpPost]
    public async Task<ActionResult<TodoItem>> AddTodo(TodoItem todo)
    {
        /* 舊POST方法
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, todo);
        */

        // 確保使用者存在，避免 EF Core 產生重複 User
        var existingUser = await _context.Users.FindAsync(todo.UserId);
        if (existingUser == null)
        {
            return BadRequest("User does not exist.");
        }

        todo.User = existingUser; // 關聯使用者

        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, todo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodo(int id, TodoItem updatedTodo)
    {
        var existingTodo = await _context.Todos.FindAsync(id);
        if (existingTodo == null)
        {
            return NotFound();
        }

        existingTodo.Title = updatedTodo.Title;
        existingTodo.IsCompleted = updatedTodo.IsCompleted;

        await _context.SaveChangesAsync();

        return Ok(existingTodo);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<TodoItem>> DeleteTodo(int id)
    {
        var existing_item = await _context.Todos.FindAsync(id);
        if (existing_item == null)
        {
            return NotFound();
        }
        _context.Todos.Remove(existing_item);
        await _context.SaveChangesAsync();  //儲存變更
        return NoContent();
    }
}
