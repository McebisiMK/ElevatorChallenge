using ElevatorChallenge.Application.Common.Interfaces;
using ElevatorChallenge.Application.Queries.Elevators.GetElevators;
using ElevatorChallenge.Application.Queries.Elevators.GetNearest;
using ElevatorChallenge.Domain.Entities;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using static ElevatorChallenge.Application.Queries.Elevators.GetNearest.GetNearestAvailableElevatorQuery;

namespace ElevatorChallenge.Tests.UnitTests;

[TestFixture]
public class GetNearestAvailableElevatorQueryHandlerTests
{
    [Test]
    public void Validate_Given_Floor_Equal_To_Zero_Should_Fail_And_Return_Correct_Validation_Error()
    {
        //------------------------------ Arrange ------------------------------
        var elevator = Substitute.For<IElevator>();
        elevator.Status.Returns(new ElevatorStatus { Id = 1 });
        var expectedErrors = new List<string> { "Floor number must be greater than 0." };

        var nearestAvailableElevatorQuery = CreateNearestAvailableElevatorQuery(floor: 0, [elevator]);
        var validator = new GetNearestAvailableElevatorQueryValidator();

        //------------------------------ Act ----------------------------------
        var actual = validator.Validate(nearestAvailableElevatorQuery);

        //------------------------------ Assert -------------------------------
        actual.IsValid.Should().BeFalse();
        actual.Errors.Should().ContainSingle(e => e.PropertyName == "Floor");
        actual.Errors.Select(x => x.ErrorMessage).Should().BeEquivalentTo(expectedErrors);
    }

    [Test]
    public void Validate_Given_Empty_List_Of_Elevators_Should_Fail_And_Return_Correct_Validation_Error()
    {
        //------------------------------ Arrange ------------------------------
        var expectedErrors = new List<string> { "Please supply list of elevators" };

        var nearestAvailableElevatorQuery = CreateNearestAvailableElevatorQuery(floor: 1, []);
        var validator = new GetNearestAvailableElevatorQueryValidator();

        //------------------------------ Act ----------------------------------
        var actual = validator.Validate(nearestAvailableElevatorQuery);

        //------------------------------ Assert -------------------------------
        actual.IsValid.Should().BeFalse();
        actual.Errors.Should().ContainSingle(e => e.PropertyName == "Elevators");
        actual.Errors.Select(x => x.ErrorMessage).Should().BeEquivalentTo(expectedErrors);
    }

    [Test]
    public async Task Handle_Given_Multiple_Elevators_Should_Return_Closest_With_Capacity()
    {
        //-------------------------------------- Arrange --------------------------------------
        var firstElevator = Substitute.For<IElevator>();
        firstElevator.Status.Returns(new ElevatorStatus { Id = 1, CurrentFloor = 2 });
        firstElevator.HasCapacity().Returns(true);

        var secondElevator = Substitute.For<IElevator>();
        secondElevator.Status.Returns(new ElevatorStatus { Id = 2, CurrentFloor = 4 });
        secondElevator.HasCapacity().Returns(true);

        var thirdElevator = Substitute.For<IElevator>();
        thirdElevator.Status.Returns(new ElevatorStatus { Id = 3, CurrentFloor = 8 });
        thirdElevator.HasCapacity().Returns(false);

        List<IElevator> elevators = [firstElevator, secondElevator, thirdElevator];
        var nearestAvailableElevatorQuery = CreateNearestAvailableElevatorQuery(floor: 5, elevators);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetStandardElevatorsQuery>(), Arg.Any<CancellationToken>()).Returns(elevators);

        var nearestAvailableElevatorQueryHandler = CreateNearestAvailableElevatorQueryHandler(mediator);

        //-------------------------------------- Act ------------------------------------------
        var actual = await nearestAvailableElevatorQueryHandler.Handle(nearestAvailableElevatorQuery, CancellationToken.None);

        //-------------------------------------- Assert ---------------------------------------
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(secondElevator); // Closest with capacity (floor 4)
    }

    [Test]
    public async Task Handle_When_No_Elevator_Has_Capacity_Should_Return_Null()
    {
        //-------------------------------------- Arrange --------------------------------------
        var firstElevator = Substitute.For<IElevator>();
        firstElevator.Status.Returns(new ElevatorStatus { Id = 1, CurrentFloor = 2 });
        firstElevator.HasCapacity().Returns(false);

        var secondElevator = Substitute.For<IElevator>();
        secondElevator.Status.Returns(new ElevatorStatus { Id = 2, CurrentFloor = 6 });
        secondElevator.HasCapacity().Returns(false);

        List<IElevator> elevators = [firstElevator, secondElevator];
        var nearestAvailableElevatorQuery = CreateNearestAvailableElevatorQuery(floor: 3, elevators);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetStandardElevatorsQuery>(), Arg.Any<CancellationToken>()).Returns(elevators);

        var nearestAvailableElevatorQueryHandler = CreateNearestAvailableElevatorQueryHandler(mediator, numberOfElevators: 2);

        //-------------------------------------- Act ------------------------------------------
        var actual = await nearestAvailableElevatorQueryHandler.Handle(nearestAvailableElevatorQuery, CancellationToken.None);

        //-------------------------------------- Assert ---------------------------------------
        actual.Should().BeNull();
    }

    private static GetNearestAvailableElevatorQueryHandler CreateNearestAvailableElevatorQueryHandler(IMediator mediator, int numberOfElevators = 3)
    {
        var configuration = CreateConfiguration(numberOfElevators);

        return new GetNearestAvailableElevatorQueryHandler(mediator, configuration);
    }

    private static GetNearestAvailableElevatorQuery CreateNearestAvailableElevatorQuery(int floor, IList<IElevator> elevators)
    {
        return new GetNearestAvailableElevatorQuery { Floor = floor, Elevators = elevators };
    }

    private static IConfiguration CreateConfiguration(int numberOfElevators)
    {
        var inMemorySettings = new Dictionary<string, string> { { "ElevatorOptions:NumberOfStandardElevators", numberOfElevators.ToString() } };

        return new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
    }
}