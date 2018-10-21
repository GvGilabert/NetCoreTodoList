

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreTodo.Controllers;
using AspNetCoreTodo.Data;
using AspNetCoreTodo.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreTodo.Services
{
    public class TodoItemService : ITodoItemService
    {
        private readonly ApplicationDbContext _context;
        public TodoItemService(ApplicationDbContext context) { _context = context; }
        public async Task<TodoItem[]> GetIncompleteItemsAsync(ApplicationUser user, TodoQueries query) 
        { 
            var getAllIncompleteTodoItems = 
                from todo in _context.Items
                where todo.IsDone == false &&  
                      todo.UserId == user.Id
                select todo;
                
            switch((int)query)
            {
                case (int)TodoQueries.IncompleteByTitleAsc:
                    return await getAllIncompleteTodoItems.OrderBy(i => i.Title).ToArrayAsync();
                case (int)TodoQueries.IncompleteByTitleDesc:
                    return await getAllIncompleteTodoItems.OrderByDescending(i=>i.Title).ToArrayAsync();
                case (int)TodoQueries.IncompleteByDateAsc:
                    return await getAllIncompleteTodoItems.OrderBy(i=>i.DueAt).ToArrayAsync();
                case (int)TodoQueries.IncompleteByDateDesc:
                    return await getAllIncompleteTodoItems.OrderByDescending(i=>i.DueAt).ToArrayAsync();
                case (int)TodoQueries.IncompleteByAddressAsc:
                    return await getAllIncompleteTodoItems.OrderBy(i=>i.Address).ToArrayAsync();
                case (int)TodoQueries.IncompleteByAddressDesc:
                    return await getAllIncompleteTodoItems.OrderByDescending(i=>i.Address).ToArrayAsync();

                default:
                    return new TodoItem[0];
            }            
        }

        public async Task<TodoItem[]> GetIncompleteItemsWithDateAsync(ApplicationUser user,DateTimeOffset date) 
        {
             var getIncompleteByDate = 
                from todo in _context.Items
                where todo.IsDone == false &&  
                      todo.UserId == user.Id &&
                      todo.DueAt >= date &&
                      todo.DueAt <= date.AddDays(1)
                orderby todo.DueAt
                select todo;
            return await getIncompleteByDate.ToArrayAsync();
        }

        public async Task<bool> AddItemAsync(TodoItem newItem, ApplicationUser user)
        {
            if(string.IsNullOrEmpty(newItem.Address))
                throw new NoAddressException();

            newItem.Id = Guid.NewGuid(); 
            newItem.IsDone = false;
            newItem.UserId = user.Id;
            newItem.Address = newItem.Address.Trim();
            
            if(newItem.DueAt < DateTimeOffset.Now.AddDays(1) || newItem.DueAt==null)
                newItem.DueAt = DateTimeOffset.Now.AddDays(1);

            _context.Items.Add(newItem);
            var saveResult = await _context.SaveChangesAsync(); 
            return saveResult == 1;
        }

        public async Task<bool> MarkDoneAsync(Guid id, ApplicationUser user)
        {
            var item = await _context.Items.Where
                (x=> x.Id == id && x.UserId == user.Id ).SingleOrDefaultAsync();

            if(item==null) return false;

            item.IsDone = true;

            var saveResult = await _context.SaveChangesAsync();
            return saveResult == 1;
        }
    }
}

