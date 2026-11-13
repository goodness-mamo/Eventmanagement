
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagementApi.Controllers;
using EventManagementApi.Models.Entities;
using EventManagementApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EventManagementApi.Models.DTOs;

namespace EventManagementApi.Tests.Controllers;

public class OrganisersControllerTests
{
    private static Organiser CreateOrganiser(int id = 1)
    {
        return new Organiser
        {
            OrganiserId = id,
            UserId = id,
            OrganisationName = $"Organisation {id}"
        };
    }

    [Fact]
    public async Task GetOrganisers_ReturnsAllOrganisers()
    {
        using var context = TestDbContextFactory.Create();

        context.Organisers.AddRange(
            CreateOrganiser(1),
            CreateOrganiser(2));

        await context.SaveChangesAsync();

        var controller = new OrganisersController(context);

        var action = await controller.GetOrganisers();

        var organisers = Assert.IsAssignableFrom<
            IEnumerable<Organiser>>(action.Value);

        Assert.Equal(2, organisers.Count());
    }

    [Fact]
    public async Task GetOrganiser_ExistingId_ReturnsOrganiser()
    {
        using var context = TestDbContextFactory.Create();

        context.Organisers.Add(CreateOrganiser(1));
        await context.SaveChangesAsync();

        var controller = new OrganisersController(context);

        var action = await controller.GetOrganiser(1);

        var result = Assert.IsType<OkObjectResult>(action.Result);
        var organiser = Assert.IsType<Organiser>(result.Value);

        Assert.Equal(1, organiser.OrganiserId);
        Assert.Equal("Organisation 1", organiser.OrganisationName);
    }

    [Fact]
    public async Task GetOrganiser_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new OrganisersController(context);

        var action = await controller.GetOrganiser(999);

        Assert.IsType<NotFoundResult>(action.Result);
    }

    [Fact]
    public async Task CreateOrganiser_ValidOrganiser_ReturnsCreated()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new OrganisersController(context);
        var request = new CreateOrganiserRequest
        {
            UserId = 1,
            OrganisationName = "Organisation 1"
        };

        var action = await controller.CreateOrganiser(request);

        var result = Assert.IsType<CreatedAtActionResult>(
            action.Result);

        Assert.Equal(
            nameof(OrganisersController.GetOrganiser),
            result.ActionName);

        Assert.Equal(1, await context.Organisers.CountAsync());
    }

    [Fact]
    public async Task UpdateOrganiser_IdMismatch_ReturnsBadRequest()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new OrganisersController(context);
        var request = new UpdateOrganiserRequest
        {
            OrganisationName = "Updated Organisation"
        };

        var action = await controller.UpdateOrganiser(1, request);

        Assert.IsType<BadRequestObjectResult>(action);
    }

    [Fact]
    public async Task UpdateOrganiser_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new OrganisersController(context);
        var request = new UpdateOrganiserRequest
        {
            OrganisationName = "Updated Organisation"
        };

        var action = await controller.UpdateOrganiser(999, request);

        Assert.IsType<NotFoundResult>(action);
    }

    [Fact]
    public async Task UpdateOrganiser_ExistingOrganiser_ReturnsNoContent()
    {
        using var context = TestDbContextFactory.Create();

        context.Organisers.Add(CreateOrganiser(1));
        await context.SaveChangesAsync();

        var controller = new OrganisersController(context);

        var updated = CreateOrganiser(1);
        var request = new UpdateOrganiserRequest
        {
            OrganisationName = "Updated Organisation"
        };

        var action = await controller.UpdateOrganiser(999, request);

        Assert.IsType<NoContentResult>(action);

        var saved = await context.Organisers.FindAsync(1);

        Assert.NotNull(saved);
        Assert.Equal(
            "Updated Organisation",
            saved.OrganisationName);
    }

    [Fact]
    public async Task DeleteOrganiser_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new OrganisersController(context);

        var action = await controller.DeleteOrganiser(999);

        Assert.IsType<NotFoundResult>(action);
    }

    [Fact]
    public async Task DeleteOrganiser_ExistingOrganiser_ReturnsNoContent()
    {
        using var context = TestDbContextFactory.Create();

        context.Organisers.Add(CreateOrganiser(1));
        await context.SaveChangesAsync();

        var controller = new OrganisersController(context);

        var action = await controller.DeleteOrganiser(1);

        Assert.IsType<NoContentResult>(action);
        Assert.Empty(await context.Organisers.ToListAsync());
    }
}
