using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApi.Models;

namespace TodoApi.Services
{
	public class ListService
	{
		private readonly ILogger<ListService> _logger;
		private readonly TodoContext _todoContext;
		private readonly SignalRConnection _pushConnection;

		public ListService(ILogger<ListService> logger, TodoContext todoContext, SignalRConnection pushConnection)
		{
			_logger = logger;
			_todoContext = todoContext ?? throw new ArgumentNullException(nameof(todoContext));
			_pushConnection = pushConnection ?? throw new ArgumentNullException(nameof(pushConnection));
		}

		public IDictionary<int, string> GetAllLists()
		{
			return _todoContext
				.Lists
				.AsNoTracking()
				.OrderBy(l => l.Id)
				.ToDictionary(l => l.Id, l => l.Name);
		}

		public int AddList(string name)
		{
			var existing = _todoContext.Lists
					.Select(l => new { l.Id, l.Name })
					.FirstOrDefault(l => l.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			if (existing != null)
				return existing.Id;

			var entity = new TodoList()
			{
				Name = name,
			};

			_todoContext.Add(entity);
			_todoContext.SaveChanges();

			_pushConnection.SendListCreated(entity.Id, entity.Name);

			return entity.Id;
		}

		public void ChangeName(int listId, string newName)
		{
			var entity = new TodoList()
			{
				Id = listId
			};

			_todoContext.Attach(entity);
			entity.Name = newName;

			_todoContext.SaveChanges();

			_pushConnection.SendListRenamed(listId, newName);
		}

		public bool DeleteList(int listId)
		{
			var entity = new TodoList()
			{
				Id = listId,
			};

			_todoContext.Entry(entity).State = EntityState.Deleted;

			try
			{
				_todoContext.SaveChanges();
				_pushConnection.SendListDeleted(listId);
				return true;
			}
			catch (DbUpdateConcurrencyException)
			{
				return false;
			}
		}

		public string GetListName(int listId)
		{
			return _todoContext.Lists.Find(listId)?.Name;
		}
	}
}
