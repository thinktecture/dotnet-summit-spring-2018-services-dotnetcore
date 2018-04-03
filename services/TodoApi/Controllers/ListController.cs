using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	public class ListController : Controller
	{
		private readonly ListService _lists;

		public ListController(ListService lists)
		{
			_lists = lists;
		}

		// GET api/list
		/// <summary>
		/// Returns an dictionary of available Todo lists and their IDs
		/// </summary>
		/// <returns>A dictionary, the keys are the ids and the values are the names of the available todo lists.</returns>
		[HttpGet]
		public IActionResult Get()
		{
			return Ok(_lists.GetAllLists());
		}
		
		// GET api/list
		/// <summary>
		/// Returns the name of a Todo list from its id
		/// </summary>
		/// <returns>The name of the list with the given id</returns>
		[HttpGet("{listId}")]
		public IActionResult Get(int listId)
		{
			return Ok(new ValueViewModel() {
				Value = _lists.GetListName(listId),
			});
		}

		// POST api/list
		[HttpPost]
		public IActionResult Post([FromBody] ValueViewModel data)
		{
			return Ok(new IdViewModel()
			{
				Id = _lists.AddList(data.Value),
			});
		}

		// PUT api/values/5
		[HttpPut("{listId}")]
		public IActionResult Put(int listId, [FromBody]  ValueViewModel data)
		{
			_lists.ChangeName(listId, data.Value);
			return Ok();
		}

		// DELETE api/list/5
		[HttpDelete("{listId}")]
		public IActionResult Delete(int listId)
		{
			if (_lists.DeleteList(listId))
				return Ok();

			return NotFound();
		}
	}
}