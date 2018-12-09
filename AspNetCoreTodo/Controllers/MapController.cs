using System;
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
    public class MapController:Controller
    {
         private readonly ITodoItemService _todoItemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration Configuration;
        public MapController(ITodoItemService todoItemService, UserManager <ApplicationUser> userManager, IConfiguration config)
        {
            _todoItemService = todoItemService;
            _userManager = userManager;
            Configuration = config;

        }

         public async Task<IActionResult> Index(DateTime date)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if(currentUser==null) return Challenge();

            if (date==DateTime.MinValue)
                date=DateTime.Now;
            
            var items = await _todoItemService.GetIncompleteItemsWithDateAsync(currentUser,date);
            GetMapInfo map = new GetMapInfo(Configuration);
            var mapQ = (from itemQ in items select itemQ.LatLong.ToString()).ToArray();
            
            var model = new TodoViewModel()
            {
                Items = items,
                Me = currentUser.UserName,
                Map = map.CallApiMap(mapQ)
            };

            return View(model);
        }
    }
}


       
        
        

       