using ElevatorChallenge.Application.Common.Behaviors;
using ElevatorChallenge.Application.Common.Interfaces;
using ElevatorChallenge.Application.Queries.Elevators.GetElevators;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ElevatorChallenge.Tests.UnitTests;

[TestFixture]
public class ValidationBehaviorTests
{
    private ServiceProvider serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddValidatorsFromAssemblyContaining<IApplication>();
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<IApplication>(); });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register Substitute for IElevatorService
        var elevatorService = Substitute.For<IElevatorService>();
        elevatorService.GetStandardElevators(Arg.Any<int>()).Returns([Substitute.For<IElevator>()]);

        services.AddSingleton(elevatorService);
        serviceProvider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        serviceProvider?.Dispose();
    }

    [Test]
    public async Task Send_Given_Invalid_Count_Should_Throw_ValidationException()
    {
        //------------------------------- Arrange -----------------------------
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var standardElevatorsQuery = new GetStandardElevatorsQuery { Count = 0 };

        //------------------------------- Act ---------------------------------
        Func<Task> action = async () => await mediator.Send(standardElevatorsQuery);

        //------------------------------- Assert ------------------------------
        await action.Should().ThrowExactlyAsync<ValidationException>()
                .Where(ex => ex.Errors.Any(e => e.PropertyName == "Count" && e.ErrorMessage.Contains("must be greater than 1"))); ;
    }

    [Test]
    public async Task Send_Given_Valid_Count_Should_Succeed()
    {
        //-------------------- Arrange ----------------------
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var standardElevatorsQuery = new GetStandardElevatorsQuery { Count = 1 };

        //-------------------- Act -------------------------
        var actual = await mediator.Send(standardElevatorsQuery);

        //-------------------- Assert ----------------------
        actual.Should().HaveCount(1);
    }
}