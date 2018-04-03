namespace TodoApi.Models
{
	public class TodoItem
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public bool Done { get; set; }

		public int TodoListId { get; set; }
		public TodoList List { get; set; }
	}
}
