
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagementApi.Controllers;
using EventManagementApi.Data;
using EventManagementApi.DTOs.Auth;
using EventManagementApi.Models.Entities;
using EventManagementApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventManagementApi.Tests.Controllers;

public class EnrolmentsControllerTests
{
    private static Enrolments CreateEnrolment(int id = 1)
    {
        return new Enrolments
        {
            EnrolmentId = id,
            EventId = 1,
            CategoryId = 1,
            ParticipantId = 1,
            EnrolledAt = DateTime.UtcNow,
            Status = "Registered",
            RaceNumber = $"R{id:000}"
        };
    }

    private static async Task SeedReferences(AppDbContext context)
    {
        context.EventCategories.Add(new EventCategory
        {
            EventId = 1,
            CategoryId = 1
        });

        context.Participants.Add(new Participant
        {
            ParticipantId = 1,
            UserId = 1,
            DateOfBirth = new DateTime(2000, 1, 1),
            EmergencyContactName = "Test Contact",
            EmergencyContactNumber = "0712345678"
        });

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetEnrolments_ReturnsAllEnrolments()
    {
        using var context = TestDbContextFactory.Create();

        context.Enrolments.AddRange(
            CreateEnrolment(1),
            CreateEnrolment(2));

        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);

        var action = await controller.GetEnrolments();

        var enrolments = Assert.IsAssignableFrom<
            IEnumerable<Enrolments>>(action.Value);

        Assert.Equal(2, enrolments.Count());
    }

    [Fact]
    public async Task GetEnrolment_ExistingId_ReturnsEnrolment()
    {
        using var context = TestDbContextFactory.Create();

        context.Enrolments.Add(CreateEnrolment(1));
        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);

        var action = await controller.GetEnrolment(1);

        var result = Assert.IsType<OkObjectResult>(action.Result);
        var enrolment = Assert.IsType<Enrolments>(result.Value);

        Assert.Equal(1, enrolment.EnrolmentId);
    }

    [Fact]
    public async Task GetEnrolment_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new EnrolmentsController(context);

        var action = await controller.GetEnrolment(999);

        Assert.IsType<NotFoundResult>(action.Result);
    }

    [Fact]
    public async Task CreateEnrolment_MissingEventCategory_ReturnsBadRequest()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new EnrolmentsController(context);

        var request = new CreateEnrolmentRequest
        {
            EventId = 1,
            CategoryId = 1
        };

        var action = await controller.CreateEnrolment(request);

        var result = Assert.IsType<BadRequestObjectResult>(action.Result);

        Assert.Contains("category", result.Value?.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateEnrolment_MissingParticipant_ReturnsBadRequest()
    {
        using var context = TestDbContextFactory.Create();

        context.EventCategories.Add(new EventCategory
        {
            EventId = 1,
            CategoryId = 1
        });

        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);

        var request = new CreateEnrolmentRequest
        {
            EventId = 1,
            CategoryId = 1
        };

        var action = await controller.CreateEnrolment(request);

        var result = Assert.IsType<BadRequestObjectResult>(action.Result);

        Assert.Contains("Participant", result.Value?.ToString());
    }

    [Fact]
    public async Task CreateEnrolment_Duplicate_ReturnsConflict()
    {
        using var context = TestDbContextFactory.Create();

        await SeedReferences(context);
        context.Enrolments.Add(CreateEnrolment(1));
        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);

        var request = new CreateEnrolmentRequest
        {
            EventId = 1,
            CategoryId = 1
        };

        var action = await controller.CreateEnrolment(request);

        Assert.IsType<ConflictObjectResult>(action.Result);
    }

    [Fact]
    public async Task CreateEnrolment_ValidData_ReturnsCreated()
    {
        using var context = TestDbContextFactory.Create();

        await SeedReferences(context);

        var controller = new EnrolmentsController(context);

        var request = new CreateEnrolmentRequest
        {
            EventId = 1,
            CategoryId = 1
        };

        var action = await controller.CreateEnrolment(request);

        var result = Assert.IsType<CreatedAtActionResult>(action.Result);

        Assert.Equal(
            nameof(EnrolmentsController.GetEnrolment),
            result.ActionName);

        Assert.Equal(1, await context.Enrolments.CountAsync());

        var saved = await context.Enrolments.SingleAsync();

        Assert.NotEqual(default, saved.EnrolledAt);
    }

    [Fact]
    public async Task UpdateEnrolment_IdMismatch_ReturnsBadRequest()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new EnrolmentsController(context);

        var action = await controller.UpdateEnrolment(
            1, CreateEnrolment(2));

        Assert.IsType<BadRequestObjectResult>(action);
    }

    [Fact]
    public async Task UpdateEnrolment_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new EnrolmentsController(context);

        var action = await controller.UpdateEnrolment(
            999, CreateEnrolment(999));

        Assert.IsType<NotFoundResult>(action);
    }

    [Fact]
    public async Task UpdateEnrolment_ValidData_ReturnsNoContent()
    {
        using var context = TestDbContextFactory.Create();

        await SeedReferences(context);
        context.Enrolments.Add(CreateEnrolment(1));
        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);

        var updated = CreateEnrolment(1);
        updated.Status = "Confirmed";
        updated.RaceNumber = "R555";

        var action = await controller.UpdateEnrolment(1, updated);

        Assert.IsType<NoContentResult>(action);

        var saved = await context.Enrolments.FindAsync(1);

        Assert.NotNull(saved);
        Assert.Equal("Confirmed", saved.Status);
        Assert.Equal("R555", saved.RaceNumber);
    }

    [Fact]
    public async Task DeleteEnrolment_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new EnrolmentsController(context);

        var action = await controller.DeleteEnrolment(999);

        Assert.IsType<NotFoundResult>(action);
    }

    [Fact]
    public async Task DeleteEnrolment_ExistingId_ReturnsNoContent()
    {
        using var context = TestDbContextFactory.Create();

        context.Enrolments.Add(CreateEnrolment(1));
        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);

        var action = await controller.DeleteEnrolment(1);

        Assert.IsType<NoContentResult>(action);
        Assert.Empty(await context.Enrolments.ToListAsync());
    }
}
