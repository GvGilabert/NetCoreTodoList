using System;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreTodo.Controllers;
using AspNetCoreTodo.Data;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetCoreTodo.UnitTests
{
    public class TodoItemServiceShould
    {

        public static ApplicationUser CreateFakeUsers(int id)
        {
            return new ApplicationUser
            {
                Id = "fake-"+id,
                UserName = "fake"+id+"@example.com"
            };
        }
        public static TodoItem CreateFakeItem(string title,string address)
        {
            return new TodoItem
            {
                Title = title,
                Address = address
            };
        }

        [Fact]
        public async Task AddNewItemAsIncompleteWithDueDate()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;

            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var fakeUser = CreateFakeUsers(0);
                await service.AddItemAsync(CreateFakeItem("Testing?","Sarandi 2020"), fakeUser);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.Items.CountAsync();
                Assert.Equal(1, itemsInDatabase);

                var item = await context.Items.FirstAsync();
                Assert.Equal("Testing?", item.Title);
                Assert.False(item.IsDone);

                var difference = DateTimeOffset.Now.AddDays(1) - item.DueAt;
                Assert.True(difference < TimeSpan.FromSeconds(1));
            }
        }
        [Fact]
        public async Task MarkDoneWrongId()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;
            var fakeUser = CreateFakeUsers(0);
            var SecondFakeUser = CreateFakeUsers(1);
            
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);    
                await service.AddItemAsync(CreateFakeItem("fake","sarandi 2020"), fakeUser);
            }
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var item = await context.Items.FirstAsync();
                Assert.False(await service.MarkDoneAsync(item.Id,SecondFakeUser));
            }
        }
        [Fact]
        public async Task MarkDoneCorrectItem()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;
            var fakeUser = CreateFakeUsers(0);
            
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                await service.AddItemAsync(CreateFakeItem("fake","sarandi 2020"), fakeUser);   
            }
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                var item = await context.Items.FirstAsync();
                Assert.True(await service.MarkDoneAsync(item.Id,fakeUser));
            }
        }
        [Fact]
        public async Task GetItemIncompleteAsycOfTheCorrectUser()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;
            var fakeUser = CreateFakeUsers(0);
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                await service.AddItemAsync(CreateFakeItem("fake","sarandi 2020"), fakeUser);   
            }
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var item = await context.Items.FirstAsync();
                TodoItem[] todoItem = await service.GetIncompleteItemsAsync(fakeUser, TodoQueries.IncompleteByTitleAsc);
                Assert.True(todoItem[0].UserId == "fake-0");
            }

        }
        [Fact]
        public async Task AddNewItemWithAddressShouldAddItCorrectly()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;
            var fakeUser = CreateFakeUsers(0);
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                await service.AddItemAsync(CreateFakeItem("fake","sarandi 2020"), fakeUser);
            }
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                var item = await context.Items.FirstAsync();
                TodoItem[] todoItem = await service.GetIncompleteItemsAsync(fakeUser, TodoQueries.IncompleteByTitleAsc);
                Assert.True(todoItem[0].UserId == "fake-0");
            }
        }
        [Fact]
        public async Task AddNewItemWithoutAddressShouldFail()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem2").Options;
            var fakeUser = CreateFakeUsers(0);
            try
            {
                using (var context = new ApplicationDbContext(options))
                {         
                    var service = new TodoItemService(context);
                    await service.AddItemAsync(CreateFakeItem("fake",""), fakeUser);
                }
                throw new Exception();
            }
            catch (NoAddressException)
            {
                using (var context = new ApplicationDbContext(options))
                {         
                    var service = new TodoItemService(context);
                    TodoItem[] todoItem = await service.GetIncompleteItemsAsync(fakeUser, TodoQueries.IncompleteByTitleAsc);
                    Assert.Empty(todoItem);
                }
            }      
        }
        [Fact]
        public async Task GetIncompleteItemsAsyncSortByTitleWithAscOrderShouldReturnElementsCorrectly()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem3").Options;
            var fakeUser = CreateFakeUsers(0);
            string [] titles = new string[]{"Sarasa","Carnasa","Mamasa","Fafasa","Papasa","Zarnasa","Arnasa","01asa","Babasa","4asa"};
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);               
                foreach(var item in titles)
                {
                    await service.AddItemAsync(CreateFakeItem(item,"Calle "+item), fakeUser);
                }
            }
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context); 
                TodoItem[] todoItem = await service.GetIncompleteItemsAsync(fakeUser, TodoQueries.IncompleteByTitleAsc);
                Array.Sort(titles);
                for(int i=0;i<titles.Length;i++)
                {
                    if(titles[i]!=todoItem[i].Title)
                        Assert.True(false);
                }
                Assert.True(true);
            }      
        }
        [Fact]
        public async Task GetIncompleteItemsAsyncSortByTitleWithDescOrderShouldReturnElementsCorrectly()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem4").Options;
            var fakeUser = CreateFakeUsers(0);
            string [] titles = new string[]{"Sarasa","Carnasa","Mamasa","Fafasa","Papasa","Zarnasa","Arnasa","01asa","Babasa","4asa"}; 
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);        
                foreach(var item in titles)
                {
                    await service.AddItemAsync(CreateFakeItem(item,"Calle "+item), fakeUser);
                }
            }
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                TodoItem[] todoItem = await service.GetIncompleteItemsAsync(fakeUser, TodoQueries.IncompleteByTitleDesc);
                Array.Sort(titles);
                Array.Reverse(titles);
                for(int i=0;i<titles.Length;i++)
                {
                    if(titles[i]!=todoItem[i].Title)
                        Assert.True(false);
                }
                Assert.True(true);
            }      
        }
        [Fact]
        public async Task GetIncompleteItemsAsyncSortByDateWithAscOrderShouldReturnElementsCorrectly()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem5").Options;
            var fakeUser = CreateFakeUsers(0);
            string [] titles = new string[]{"Sarasa","Carnasa","Mamasa","Fafasa","Papasa","Zarnasa","Arnasa","01asa","Babasa","4asa"};
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
              
                Random rnd = new Random();
                foreach(var item in titles)
                {
                    await service.AddItemAsync(new TodoItem {Title = item, Address ="Calle "+item,DueAt=DateTime.Today.AddDays(rnd.Next(-1000,1000))}, fakeUser);
                } 
            }
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                TodoItem[] todoItem = await service.GetIncompleteItemsAsync(fakeUser, TodoQueries.IncompleteByDateAsc);
                
                var sortedByDate = from items in todoItem orderby items.DueAt ascending select items;   
                TodoItem[] sortedList = sortedByDate.ToArray();
                
                for(int i=0;i<todoItem.Length;i++)
                {
                    if(sortedList[i].DueAt!=todoItem[i].DueAt)
                        Assert.True(false);
                }
                Assert.True(true);
            }      
        }
        [Fact]
        public async Task GetIncompleteItemsAsyncSortByDateWithDescOrderShouldReturnElementsCorrectly()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem6").Options;
            var fakeUser = CreateFakeUsers(0);
            string [] titles = new string[]{"Sarasa","Carnasa","Mamasa","Fafasa","Papasa","Zarnasa","Arnasa","01asa","Babasa","4asa"};
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                Random rnd = new Random();
                foreach(var item in titles)
                {
                    await service.AddItemAsync(new TodoItem {Title = item, Address ="Calle "+item,DueAt=DateTime.Today.AddDays(rnd.Next(-1000,1000))}, fakeUser);
                }
            }
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                TodoItem[] todoItem = await service.GetIncompleteItemsAsync(fakeUser, TodoQueries.IncompleteByDateDesc);
                var sortedByDate = from items in todoItem orderby items.DueAt descending select items;   
                TodoItem[] sortedList = sortedByDate.ToArray();
                
                for(int i=0;i<todoItem.Length;i++)
                {
                    if(sortedList[i].DueAt!=todoItem[i].DueAt)
                        Assert.True(false);
                }
                Assert.True(true);
            }
        }      
        [Fact]
        public async Task GetIncompleteItemsAsyncSortByAddressWithAscOrderShouldReturnElementsCorrectly()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem7").Options;
            var fakeUser = CreateFakeUsers(0);
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                 string [] titles = new string[]{"Sarasa","Carnasa","Mamasa","Fafasa","Papasa","Zarnasa","Arnasa","01asa","Babasa","4asa"};
                Random rnd = new Random();
                foreach(var item in titles)
                {
                    await service.AddItemAsync(new TodoItem {Title = item, Address ="Calle "+item,DueAt=DateTime.Today.AddDays(rnd.Next(-1000,1000))}, fakeUser);
                }
            }
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                TodoItem[] todoItem = await service.GetIncompleteItemsAsync(fakeUser, TodoQueries.IncompleteByAddressAsc);
                var sortedByAddress = from items in todoItem orderby items.Address ascending select items;   
                TodoItem[] sortedList = sortedByAddress.ToArray();
                
                for(int i=0;i<todoItem.Length;i++)
                {
                    if(sortedList[i].Address!=todoItem[i].Address)
                        Assert.True(false);
                }
                Assert.True(true);
            }
        }
        [Fact]
        public async Task GetIncompleteItemsAsyncSortByAddressWithDescOrderShouldReturnElementsCorrectly()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem8").Options;
            var fakeUser = CreateFakeUsers(0);
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                string [] titles = new string[]{"Sarasa","Carnasa","Mamasa","Fafasa","Papasa","Zarnasa","Arnasa","01asa","Babasa","4asa"};
                Random rnd = new Random();
                foreach(var item in titles)
                {
                    await service.AddItemAsync(new TodoItem {Title = item, Address ="Calle "+item,DueAt=DateTime.Today.AddDays(rnd.Next(-1000,1000))}, fakeUser);
                }
            }
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                TodoItem[] todoItem = await service.GetIncompleteItemsAsync(fakeUser, TodoQueries.IncompleteByAddressDesc);
                var sortedByAddress = from items in todoItem orderby items.Address descending select items;   
                TodoItem[] sortedList = sortedByAddress.ToArray();
                
                for(int i=0;i<todoItem.Length;i++)
                {
                    if(sortedList[i].Address!=todoItem[i].Address)
                        Assert.True(false);
                }
                Assert.True(true);
            }
        }            
        [Fact]
        public async Task GetIncompleteItemsAsyncForGivenDateShouldReturnElementsCorrectly()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem9").Options;
            var fakeUser = CreateFakeUsers(0);
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                int counter = 4;
                while(counter>0)
                {
                    if(counter%2==0)
                        await service.AddItemAsync(new TodoItem {Title = "Tarea"+counter, 
                                                                Address ="Calle Sarasa 202"+counter+" CABA",
                                                                DueAt= DateTime.Now.AddDays(counter)}, fakeUser);
                    else
                        await service.AddItemAsync(new TodoItem {Title = "Tarea"+counter, 
                                                                Address ="Calle Sarasa 202"+counter+" CABA",
                                                                DueAt= DateTime.Now.AddHours(counter)}, fakeUser);
                    counter--;
                }
            }
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                TodoItem[] todoItem = await service.GetIncompleteItemsWithDateAsync(fakeUser,DateTime.Now);
                Assert.True(todoItem.Length==2);
            }
        } 
        [Fact]
        public async Task GetIncompleteItemsAsyncForGivenDateWithEmptyResultShouldReturnEmpty()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem10").Options;
            var fakeUser = CreateFakeUsers(0);
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);  
                await service.AddItemAsync(new TodoItem {Title = "Tarea1", Address ="Calle Sarasa 2020 CABA",DueAt= DateTime.Now.AddDays(5)}, fakeUser);
            }
            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);  
                TodoItem[] todoItem = await service.GetIncompleteItemsWithDateAsync(fakeUser,DateTime.Now);
                Assert.Empty(todoItem);      
            }
        }
    }
}