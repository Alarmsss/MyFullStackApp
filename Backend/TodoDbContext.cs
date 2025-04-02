using Microsoft.EntityFrameworkCore;
using MyFullStackApp.Backend.Models;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {} //***

    public DbSet<User> Users { get; set; }
    public DbSet<TodoItem> Todos { get; set; }

    //OnModelCreating: 這個方法在 資料庫建立時 (migration) 會被執行，讓開發者可以設定 資料表的關聯。
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>()
            .HasOne(t => t.User)
            .WithMany(u => u.Todos)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade); //某個 User 被刪除時，其 所有的 TodoItem 也會自動刪除（這就是 CASCADE 行為）。

        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedNever(); // 確保 User ID 來自外部 (例如 Google)
    }
}
