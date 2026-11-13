
using EventManagementApi.Controllers;
using EventManagementApi.Models.Entities;
using EventManagementApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EventManagementApi.Tests.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        using var context = TestDbContextFactory.Create();

        context.Users.AddRange(
            CreateUser(1, "Alex", "Smith", "alex@example.com"),
            CreateUser(2, "Sam", "Jones", "sam@example.com")
        );

        await context.SaveChangesAsync();

        var controller = new UsersController(context);

        var action = await controller.GetUsers();

        var result = Assert.IsType<OkObjectResult>(action.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<User>>(
            result.Value);

        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task GetUser_ExistingId_ReturnsUser()
    {
        using var context = TestDbContextFactory.Create();

        context.Users.Add(
            CreateUser(1, "Alex", "Smith", "alex@example.com"));

        await context.SaveChangesAsync();

        var controller = new UsersController(context);

        var action = await controller.GetUser(1);

        var result = Assert.IsType<OkObjectResult>(action.Result);
        var user = Assert.IsType<User>(result.Value);

        Assert.Equal(1, user.UserId);
        Assert.Equal("Alex", user.FirstName);
        Assert.Equal("alex@example.com", user.Email);
    }

    [Fact]
    public async Task GetUser_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new UsersController(context);

        var action = await controller.GetUser(999);

        Assert.IsType<NotFoundResult>(action.Result);
    }

    private static User CreateUser(
        int id,
        string firstName,
        string lastName,
        string email)
    {
        return new User
        {
            UserId = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = "test-hash",
            Role = "Participant",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}

