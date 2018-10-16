using System;
using System.Threading.Tasks;
using AspNetCoreTodo.Data;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetCoreTodo.UnitTests
{
    public class TodoItemServiceShould
    {
        [Fact]
        public async Task AddNewItemAsIncompleteWithDueDate()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;

            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@example.com"
                };

                await service.AddItemAsync(new TodoItem {Title = "Testing?"}, fakeUser);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.Items.CountAsync();
                Assert.Equal(1, itemsInDatabase);

                var item = await context.Items.FirstAsync();
                Assert.Equal("Testing?", item.Title);
                Assert.Equal(false, item.IsDone);

                // Item should be due 3 days from now (give or take a second)
                var difference = DateTimeOffset.Now.AddDays(3) - item.DueAt;
                Assert.True(difference < TimeSpan.FromSeconds(1));
            }
        }

        [Fact]
        public async Task MarkDoneWrongId()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;
            using (var context = new ApplicationDbContext(options))
            {
                
                var service = new TodoItemService(context);
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@example.com"
                };
                var SecondFakeUser = new ApplicationUser
                {
                    Id = "fake-001",
                    UserName = "fake2@example.com"
                };

                await service.AddItemAsync(new TodoItem {Title = "Testing?"}, fakeUser);
                var item = await context.Items.FirstAsync();
                Assert.False(await service.MarkDoneAsync(item.Id,SecondFakeUser));
            }
        }

        [Fact]
        public async Task MarkDoneCorrectItem()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;

            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@example.com"
                };

                await service.AddItemAsync(new TodoItem {Title = "Testing?"}, fakeUser);
                var item = await context.Items.FirstAsync();
                Assert.True(await service.MarkDoneAsync(item.Id,fakeUser));
            }
        }

          [Fact]
        public async Task GetItemIncompleteAsycOfTheCorrectUser()
        {
            var options = new DbContextOptionsBuilder<AspNetCoreTodo.Data.ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;

            using (var context = new ApplicationDbContext(options))
            {      
                var service = new TodoItemService(context);
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@example.com"
                };

                await service.AddItemAsync(new TodoItem {Title = "Testing?"}, fakeUser);
                var item = await context.Items.FirstAsync();
                TodoItem[] todoItem = await service.GetIncompleteItemsAsync(fakeUser);
                Assert.True(todoItem[0].UserId == "fake-000");
            }

        }
    }
}