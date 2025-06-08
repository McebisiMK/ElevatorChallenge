using ElevatorChallenge.Domain.Entities;
using ElevatorChallenge.Domain.Enums;
using ElevatorChallenge.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ElevatorChallenge.Tests.UnitTests;

[TestFixture]
public class StandardElevatorTests
{
    [Test]
    public void Constructor_Should_Set_Default_Status_And_MaxCapacity()
    {
        //------------------------- Arrange -------------------------
        var elevatorId = 1;
        var maxCapacity = 8;
        var expectedStatus = new ElevatorStatus { Id = 1, Direction = Direction.Idle };

        //------------------------- Act ----------------------------
        var elevator = CreateStandardElevator(elevatorId, maxCapacity);

        //------------------------- Assert --------------------------
        elevator.MaxCapacity.Should().Be(maxCapacity);
        elevator.PendingRequests.Should().HaveCount(0);
        elevator.Status.Should().BeEquivalentTo(expectedStatus);
    }

    [TestCase(3)]
    [TestCase(4)]
    public void HasCapacity_Given_Elevator_Has_Space_Should_Return_True(int currentPassengers)
    {
        //-------------------------------- Arrange --------------------------------
        var elevatorId = 1;
        var maxCapacity = 5;
        var elevator = CreateStandardElevator(elevatorId, maxCapacity);

        elevator.Status.PassengerCount = currentPassengers;

        //-------------------------------- Act ------------------------------------
        var actual = elevator.HasCapacity();

        //-------------------------------- Assert ---------------------------------
        actual.Should().BeTrue();
        elevator.MaxCapacity.Should().Be(maxCapacity);
        elevator.PendingRequests.Should().HaveCount(0);
    }

    [TestCase(5)]
    [TestCase(6)]
    public void HasCapacity_Given_Elevator_Has_No_Space_Should_Return_False(int currentPassengers)
    {
        //-------------------------------- Arrange --------------------------------
        var elevatorId = 1;
        var maxCapacity = 5;
        var elevator = CreateStandardElevator(elevatorId, maxCapacity);

        elevator.Status.PassengerCount = currentPassengers;

        //-------------------------------- Act ------------------------------------
        var actual = elevator.HasCapacity();

        //-------------------------------- Assert ---------------------------------
        actual.Should().BeFalse();
        elevator.MaxCapacity.Should().Be(maxCapacity);
        elevator.PendingRequests.Should().HaveCount(0);
    }

    [Test]
    public void Move_Given_No_Requests_Should_Return_Direction_Idle()
    {
        //-------------------------------- Arrange --------------------------------
        var elevatorId = 1;
        var elevator = CreateStandardElevator(elevatorId);

        //-------------------------------- Act ------------------------------------
        elevator.Move();

        //-------------------------------- Assert ---------------------------------
        elevator.Status.IsMoving.Should().BeFalse();
        elevator.PendingRequests.Should().HaveCount(0);
        elevator.Status.Direction.Should().Be(Direction.Idle);
    }

    [Test]
    public void Move_Given_Request_To_Upper_Floor_Should_Move_Up_And_Stop_Once_Arrived()
    {
        //-------------------------------- Arrange --------------------------------
        var elevatorId = 1;
        var elevator = CreateStandardElevator(elevatorId, startFloor: 0);

        elevator.AddRequest(floor: 2);

        //-------------------------------- Act ------------------------------------
        elevator.Move(); // should go to floor 1

        //-------------------------------- Assert ---------------------------------
        elevator.Status.IsMoving.Should().BeTrue();
        elevator.Status.CurrentFloor.Should().Be(1);
        elevator.PendingRequests.Should().HaveCount(1);
        elevator.Status.Direction.Should().Be(Direction.Up);

        elevator.Move(); // should go to floor 2 and stop

        //-------------------------------- Assert ---------------------------------
        elevator.Status.CurrentFloor.Should().Be(2);
        elevator.Status.IsMoving.Should().BeFalse();
        elevator.PendingRequests.Should().HaveCount(0);
        elevator.Status.Direction.Should().Be(Direction.Idle);
    }

    [Test]
    public void Move_Given_Request_To_Lower_Floor_Should_Move_Down_And_Stop_Once_Arrived()
    {
        //-------------------------------- Arrange --------------------------------
        var elevatorId = 1;
        var elevator = CreateStandardElevator(elevatorId, startFloor: 3);

        elevator.AddRequest(floor: 1);

        //-------------------------------- Act ------------------------------------
        elevator.Move(); // should go to floor 2

        //-------------------------------- Assert ---------------------------------
        elevator.Status.IsMoving.Should().BeTrue();
        elevator.Status.CurrentFloor.Should().Be(2);
        elevator.PendingRequests.Should().HaveCount(1);
        elevator.Status.Direction.Should().Be(Direction.Down);

        elevator.Move(); // should go to floor 1 and stop

        //-------------------------------- Assert ---------------------------------
        elevator.Status.CurrentFloor.Should().Be(1);
        elevator.Status.IsMoving.Should().BeFalse();
        elevator.PendingRequests.Should().HaveCount(0);
        elevator.Status.Direction.Should().Be(Direction.Idle);
    }

    [Test]
    public void AddRequest_Should_Not_Add_Duplicate_Floor()
    {
        //-------------------------------- Arrange --------------------------------
        var elevatorId = 1;
        var elevator = CreateStandardElevator(elevatorId, startFloor: 0);

        elevator.AddRequest(floor: 1);
        elevator.AddRequest(floor: 1); // duplicate

        //-------------------------------- Act ------------------------------------
        elevator.Move(); // should go to floor 1

        //-------------------------------- Assert ---------------------------------
        elevator.Status.CurrentFloor.Should().Be(1);
        elevator.Status.IsMoving.Should().BeFalse(); // Should stop and serve both requests in one
        elevator.Status.Direction.Should().Be(Direction.Idle); // Stop, both requests are served
        elevator.PendingRequests.Should().HaveCount(0); // No more requests
    }

    [Test]
    public void Move_Given_Request_On_Current_Floor_Should_Immediately_Stop()
    {
        //-------------------------------- Arrange --------------------------------
        var elevatorId = 1;
        var elevator = CreateStandardElevator(elevatorId, startFloor: 2);

        elevator.AddRequest(floor: 2);

        //-------------------------------- Act ------------------------------------
        elevator.Move();

        //-------------------------------- Assert ---------------------------------
        elevator.Status.CurrentFloor.Should().Be(2);
        elevator.Status.IsMoving.Should().BeFalse();
        elevator.PendingRequests.Should().HaveCount(0);
        elevator.Status.Direction.Should().Be(Direction.Idle);
    }

    private static StandardElevator CreateStandardElevator(int id, int maxCapacity = 5, int startFloor = 0)
    {
        var configuration = CreateConfiguration(maxCapacity);

        return new StandardElevator(configuration, id, startFloor);
    }

    private static IConfiguration CreateConfiguration(int maxCapacity)
    {
        var inMemorySettings = new Dictionary<string, string> { { "ElevatorOptions:ElevatorDefaultMaxCapacity", maxCapacity.ToString() } };

        return new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
    }
}