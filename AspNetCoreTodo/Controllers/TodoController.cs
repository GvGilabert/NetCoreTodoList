using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AspNetCoreTodo.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ITodoItemService _todoItemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration Configuration;

        public TodoController(ITodoItemService todoItemService, UserManager <ApplicationUser> userManager, IConfiguration config)
        {
            _todoItemService = todoItemService;
            _userManager = userManager;
            Configuration = config;
        }

        public async Task<IActionResult> Index(string sortQuery)
        {
            if(ViewBag.addressBag ==null)
                {
                    ViewBag.addressBag = new AddressViewModel();
                }  
            var currentUser = await _userManager.GetUserAsync(User);
            if(currentUser==null) return Challenge();
            
            if(sortQuery ==null)
                sortQuery= "IncompleteByTitleAsc";
            TodoQueries query = (TodoQueries)Enum.Parse(typeof(TodoQueries) ,sortQuery);
            
            var items = await _todoItemService.GetIncompleteItemsAsync(currentUser, query);

            var model = new TodoViewModel()
            {
                Items = items,
                Me = currentUser.UserName
            };

            return View(model);
        }
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(TodoItem newItem)
        {

            if (!ModelState.IsValid) 
            { 
                return RedirectToAction("Index"); 
            }
            var	currentUser	= await	_userManager.GetUserAsync(User);
            
            if	(currentUser ==	null) 
                return Challenge();


            var successful = await _todoItemService.AddItemAsync(newItem, currentUser);

            if (!successful) { return BadRequest("Could	not	add	item."); }
            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDone(Guid id)
        {
            if (id == Guid.Empty) { return RedirectToAction("Index"); }
            var	currentUser	= await _userManager.GetUserAsync(User);
            if	(currentUser ==	null)
                return	Challenge();
	
            var successful = await _todoItemService.MarkDoneAsync(id, currentUser); 
            if (!successful) 
            { 
                return BadRequest("Could not mark item as done.");
            }
            return RedirectToAction("Index");
        }

        public async Task <IActionResult> AddressPartial(string address)
        {
            if(address==null)
                return BadRequest("Address field is empty");
            
            AddressViewModel model = new AddressViewModel();
            GetMapInfo map = new GetMapInfo(Configuration);
            model.info = await map.CallApiAsync(address);
            ViewBag.addressBag = model;
            return PartialView ("AddressPartial");
        }

        
        
        
        

        




    }
}