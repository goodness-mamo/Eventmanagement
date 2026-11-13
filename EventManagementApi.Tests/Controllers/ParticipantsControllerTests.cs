
using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using EventManagementApi.Controllers;
using EventManagementApi.Data;
using EventManagementApi.Models.Entities;
using EventManagementApi.Services;
using EventManagementApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventManagementApi.Tests.Controllers;

public class ParticipantsControllerTests
{
    private static BlobStorageService CreateBlobStorageService()
    {
        var blobClient = new BlobServiceClient(
            "UseDevelopmentStorage=true");

        return new BlobStorageService(blobClient);
    }

    private static User CreateUser(int id = 1)
    {
        return new User
        {
            UserId = id,
            FirstName = "Alex",
            LastName = "Smith",
            Email = $"alex{id}@example.com",
            PasswordHash = "test-hash",
            Role = "Participant",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Participant CreateParticipant(
        int participantId = 1,
        int userId = 1)
    {
        return new Participant
        {
            ParticipantId = participantId,
            UserId = userId,
            DateOfBirth = new DateTime(2000, 5, 15),
            EmergencyContactName = "Jane Smith",
            EmergencyContactNumber = "0712345678"
        };
    }

    private static async Task SeedParticipantAsync(
        AppDbContext context)
    {
        var user = CreateUser();

        context.Users.Add(user);
        context.Participants.Add(CreateParticipant());

        await context.SaveChangesAsync();
    }

    private static ParticipantsController CreateController(
        AppDbContext context)
    {
        return new ParticipantsController(
            context,
            CreateBlobStorageService());
    }

    [Fact]
    public async Task GetParticipants_ReturnsOk()
    {
        using var context = TestDbContextFactory.Create();
        await SeedParticipantAsync(context);

        var controller = CreateController(context);

        var action = await controller.GetParticipants();

        var result = Assert.IsType<OkObjectResult>(action);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetParticipant_ExistingId_ReturnsOk()
    {
        using var context = TestDbContextFactory.Create();
        await SeedParticipantAsync(context);

        var controller = CreateController(context);

        var action = await controller.GetParticipant(1);

        var result = Assert.IsType<OkObjectResult>(action);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetParticipant_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = CreateController(context);

        var action = await controller.GetParticipant(999);

        Assert.IsType<NotFoundResult>(action);
    }

    [Fact]
    public async Task CreateParticipant_ValidUser_ReturnsCreated()
    {
        using var context = TestDbContextFactory.Create();

        context.Users.Add(CreateUser());
        await context.SaveChangesAsync();

        var controller = CreateController(context);
        var participant = CreateParticipant(participantId: 0);

        var action = await controller.CreateParticipant(participant);

        Assert.IsType<CreatedAtActionResult>(action);
        Assert.Equal(1, await context.Participants.CountAsync());
    }

    [Fact]
    public async Task CreateParticipant_UnknownUser_ReturnsBadRequest()
    {
        using var context = TestDbContextFactory.Create();

        var controller = CreateController(context);
        var participant = CreateParticipant(userId: 999);

        var action = await controller.CreateParticipant(participant);

        Assert.IsType<BadRequestObjectResult>(action);
        Assert.Empty(await context.Participants.ToListAsync());
    }

    [Fact]
    public async Task CreateParticipant_DuplicateUser_ReturnsConflict()
    {
        using var context = TestDbContextFactory.Create();
        await SeedParticipantAsync(context);

        var controller = CreateController(context);
        var duplicate = CreateParticipant(participantId: 0);

        var action = await controller.CreateParticipant(duplicate);

        Assert.IsType<ConflictObjectResult>(action);
        Assert.Single(await context.Participants.ToListAsync());
    }

    [Fact]
    public async Task UpdateParticipant_IdMismatch_ReturnsBadRequest()
    {
        using var context = TestDbContextFactory.Create();

        var controller = CreateController(context);
        var participant = CreateParticipant(participantId: 2);

        var action = await controller.UpdateParticipant(1, participant);

        Assert.IsType<BadRequestObjectResult>(action);
    }

    [Fact]
    public async Task UpdateParticipant_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = CreateController(context);
        var participant = CreateParticipant(participantId: 999);

        var action = await controller.UpdateParticipant(999, participant);

        Assert.IsType<NotFoundResult>(action);
    }

    [Fact]
    public async Task UpdateParticipant_ExistingParticipant_ReturnsNoContent()
    {
        using var context = TestDbContextFactory.Create();
        await SeedParticipantAsync(context);

        var controller = CreateController(context);
        var updated = CreateParticipant();

        updated.EmergencyContactName = "Updated Contact";
        updated.EmergencyContactNumber = "0798765432";

        var action = await controller.UpdateParticipant(1, updated);

        Assert.IsType<NoContentResult>(action);

        var saved = await context.Participants.FindAsync(1);

        Assert.NotNull(saved);
        Assert.Equal("Updated Contact", saved.EmergencyContactName);
        Assert.Equal("0798765432", saved.EmergencyContactNumber);
    }

    [Fact]
    public async Task DeleteParticipant_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = CreateController(context);

        var action = await controller.DeleteParticipant(999);

        Assert.IsType<NotFoundResult>(action);
    }

    [Fact]
    public async Task DeleteParticipant_ExistingParticipant_ReturnsNoContent()
    {
        using var context = TestDbContextFactory.Create();
        await SeedParticipantAsync(context);

        var controller = CreateController(context);

        var action = await controller.DeleteParticipant(1);

        Assert.IsType<NoContentResult>(action);
        Assert.Empty(await context.Participants.ToListAsync());
    }
}
