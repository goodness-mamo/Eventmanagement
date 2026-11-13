
using System.Reflection;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using EventManagementApi.Controllers;
using EventManagementApi.Data;
using EventManagementApi.Services;
using EventManagementApi.Services.Interfaces;
using EventManagementApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EventManagementApi.Tests.Controllers;

public class EventsControllerTests
{
    private static BlobStorageService CreateBlobStorageService()
    {
        var blobClient = new BlobServiceClient(
            "UseDevelopmentStorage=true");

        return new BlobStorageService(blobClient);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        using var context = TestDbContextFactory.Create();

        var service = DispatchProxy.Create<
            IEventService, FakeEventService>();

        var controller = new EventsController(
            service,
            context,
            CreateBlobStorageService());

        var action = await controller.GetAll();

        Assert.IsType<OkObjectResult>(action.Result);
    }

    [Fact]
    public async Task GetById_UnknownEvent_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var service = DispatchProxy.Create<
            IEventService, FakeEventService>();

        var controller = new EventsController(
            service,
            context,
            CreateBlobStorageService());

        var action = await controller.GetById(999);

        Assert.IsType<NotFoundObjectResult>(action.Result);
    }

    public class FakeEventService : DispatchProxy
    {
        protected override object? Invoke(
            MethodInfo? targetMethod,
            object?[]? args)
        {
            var returnType = targetMethod!.ReturnType;

            if (returnType == typeof(Task))
            {
                return Task.CompletedTask;
            }

            if (returnType.IsGenericType &&
                returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultType = returnType.GetGenericArguments()[0];

                object? value = resultType == typeof(bool)
                    ? false
                    : null;

                return typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { value });
            }

            throw new NotSupportedException(
                $"Unsupported return type: {returnType}");
        }
    }
}
