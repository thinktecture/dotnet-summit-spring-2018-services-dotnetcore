using System.Collections.Generic;

namespace TodoApi.Models
{
	public class TodoList
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public HashSet<TodoItem> Items { get; set; } = new HashSet<TodoItem>();
	}
}
