
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagementApi.Controllers;
using EventManagementApi.Models.Entities;
using EventManagementApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventManagementApi.Tests.Controllers;

public class CategoriesControllerTests
{
    private static Categories CreateCategory(int id = 1)
    {
        return new Categories
        {
            CategoryId = id,
            CategoryName = $"Category {id}",
            Description = $"Description for category {id}",
            MinimumAge = 18,
            MaximumAge = 40
        };
    }

    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        using var context = TestDbContextFactory.Create();

        context.Categories.AddRange(
            CreateCategory(1),
            CreateCategory(2));

        await context.SaveChangesAsync();

        var controller = new CategoriesController(context);

        var action = await controller.GetCategories();

        var categories = Assert.IsAssignableFrom<
            IEnumerable<Categories>>(action.Value);

        Assert.Equal(2, System.Linq.Enumerable.Count(categories));
    }

    [Fact]
    public async Task GetCategory_ExistingId_ReturnsCategory()
    {
        using var context = TestDbContextFactory.Create();

        context.Categories.Add(CreateCategory(1));
        await context.SaveChangesAsync();

        var controller = new CategoriesController(context);

        var action = await controller.GetCategory(1);

        var result = Assert.IsType<OkObjectResult>(action.Result);
        var category = Assert.IsType<Categories>(result.Value);

        Assert.Equal(1, category.CategoryId);
        Assert.Equal("Category 1", category.CategoryName);
    }

    [Fact]
    public async Task GetCategory_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new CategoriesController(context);

        var action = await controller.GetCategory(999);

        Assert.IsType<NotFoundResult>(action.Result);
    }

    [Fact]
    public async Task CreateCategory_ValidCategory_ReturnsCreated()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new CategoriesController(context);
        var category = CreateCategory();

        var action = await controller.CreateCategory(category);

        var result = Assert.IsType<CreatedAtActionResult>(
            action.Result);

        Assert.Equal(nameof(CategoriesController.GetCategory),
            result.ActionName);

        Assert.Equal(1, await context.Categories.CountAsync());
    }

    [Fact]
    public async Task UpdateCategory_IdMismatch_ReturnsBadRequest()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new CategoriesController(context);
        var category = CreateCategory(2);

        var action = await controller.UpdateCategory(1, category);

        Assert.IsType<BadRequestObjectResult>(action);
    }

    [Fact]
    public async Task UpdateCategory_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new CategoriesController(context);
        var category = CreateCategory(999);

        var action = await controller.UpdateCategory(999, category);

        Assert.IsType<NotFoundResult>(action);
    }

    [Fact]
    public async Task UpdateCategory_ExistingCategory_ReturnsNoContent()
    {
        using var context = TestDbContextFactory.Create();

        context.Categories.Add(CreateCategory(1));
        await context.SaveChangesAsync();

        var controller = new CategoriesController(context);

        var updated = new Categories
        {
            CategoryId = 1,
            CategoryName = "Updated Category",
            Description = "Updated description",
            MinimumAge = 21,
            MaximumAge = 50
        };

        var action = await controller.UpdateCategory(1, updated);

        Assert.IsType<NoContentResult>(action);

        var saved = await context.Categories.FindAsync(1);

        Assert.NotNull(saved);
        Assert.Equal("Updated Category", saved.CategoryName);
        Assert.Equal("Updated description", saved.Description);
        Assert.Equal(21, saved.MinimumAge);
        Assert.Equal(50, saved.MaximumAge);
    }

    [Fact]
    public async Task DeleteCategory_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new CategoriesController(context);

        var action = await controller.DeleteCategory(999);

        Assert.IsType<NotFoundResult>(action);
    }

    [Fact]
    public async Task DeleteCategory_ExistingCategory_ReturnsNoContent()
    {
        using var context = TestDbContextFactory.Create();

        context.Categories.Add(CreateCategory(1));
        await context.SaveChangesAsync();

        var controller = new CategoriesController(context);

        var action = await controller.DeleteCategory(1);

        Assert.IsType<NoContentResult>(action);
        Assert.Empty(await context.Categories.ToListAsync());
    }
}
