using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApi.Models;

namespace TodoApi.Services
{
	public class ItemService
	{
		private readonly ILogger<ItemService> _logger;
		private readonly TodoContext _todoContext;
		private readonly SignalRConnection _pushConnection;

		public ItemService(ILogger<ItemService> logger, TodoContext todoContext, SignalRConnection pushConnection)
		{
			_logger = logger;
			_todoContext = todoContext ?? throw new ArgumentNullException(nameof(todoContext));
			_pushConnection = pushConnection;
		}

		public IEnumerable<ItemViewModel> GetAllItems(int listId)
		{
			return _todoContext
				.Items
				.AsNoTracking()
				.Where(i => i.TodoListId == listId)
				.OrderBy(i => i.Id)
				.Select(i => new ItemViewModel()
				{
					Id = i.Id,
					ListId = i.TodoListId,
					Text = i.Text,
					Done = i.Done,
				})
				.ToList();
		}

		public int AddItem(int listId, string text)
		{
			var existing = _todoContext.Items
					.Select(i => new { i.Id, i.Text })
					.FirstOrDefault(i => i.Text.Equals(text, StringComparison.InvariantCultureIgnoreCase));

			if (existing != null)
				return existing.Id;

			var entity = new TodoItem()
			{
				TodoListId = listId,
				Text = text,
			};

			_todoContext.Add(entity);
			_todoContext.SaveChanges();

			_pushConnection.SendItemAdded(listId, entity.Id, entity.Text);
			return entity.Id;
		}

		public void ChangeText(int listId, int itemId, string newText)
		{
			var entity = new TodoItem()
			{
				Id = itemId,
			};

			_todoContext.Attach(entity);
			entity.Text = newText;

			_todoContext.SaveChanges();
			_pushConnection.SendItemNameChanged(listId, itemId, newText);
		}

		public void ToggleItemDone(int listId, int itemId)
		{
			var entity = _todoContext.Items.Find(itemId);

			entity.Done = !entity.Done;
			_todoContext.SaveChanges();

			_pushConnection.SendItemDoneChanged(listId, itemId, entity.Done);
		}

		public bool DeleteItem(int listId, int itemId)
		{
			var entity = new TodoItem()
			{
				Id = itemId,
			};

			_todoContext.Entry(entity).State = EntityState.Deleted;

			try
			{
				_todoContext.SaveChanges();
				_pushConnection.SendItemDeleted(listId, itemId);
				return true;
			}
			catch (DbUpdateConcurrencyException)
			{
				return false;
			}
		}
	}
}
