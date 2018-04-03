using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models
{
	public class TodoContext : DbContext
	{
		public DbSet<TodoList> Lists { get; set; }
		public DbSet<TodoItem> Items { get; set; }

		public TodoContext(DbContextOptions<TodoContext> options)
			: base(options)
		{
		}
	}
}
