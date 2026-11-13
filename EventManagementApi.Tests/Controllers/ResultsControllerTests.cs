
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagementApi.Controllers;
using EventManagementApi.Models.Entities;
using EventManagementApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventManagementApi.Tests.Controllers;

public class ResultsControllerTests
{
    private static Result CreateResult(int id = 1)
    {
        return new Result
        {
            ResultId = id,
            EnrolmentId = id,
            Position = id,
            FinishTime = new System.TimeSpan(0, 30, id),
            ResultStatus = "Finished"
        };
    }

    [Fact]
    public async Task GetResults_ReturnsAllResults()
    {
        using var context = TestDbContextFactory.Create();

        context.Results.AddRange(
            CreateResult(1),
            CreateResult(2));

        await context.SaveChangesAsync();

        var controller = new ResultsController(context);

        var action = await controller.GetResults();

        var result = Assert.IsType<OkObjectResult>(action.Result);

        var results = Assert.IsAssignableFrom<IEnumerable<Result>>(
            result.Value);

        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task GetResult_ExistingId_ReturnsResult()
    {
        using var context = TestDbContextFactory.Create();

        context.Results.Add(CreateResult(1));
        await context.SaveChangesAsync();

        var controller = new ResultsController(context);

        var action = await controller.GetResult(1);

        var result = Assert.IsType<OkObjectResult>(action.Result);
        var returnedResult = Assert.IsType<Result>(result.Value);

        Assert.Equal(1, returnedResult.ResultId);
        Assert.Equal(1, returnedResult.Position);
        Assert.Equal("Finished", returnedResult.ResultStatus);
    }

    [Fact]
    public async Task GetResult_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();

        var controller = new ResultsController(context);

        var action = await controller.GetResult(999);

        Assert.IsType<NotFoundResult>(action.Result);
    }
}
