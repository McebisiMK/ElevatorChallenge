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
        var expectedStatus = new ElevatorStatus { Id = 1, Direction = Direction.Idle, CurrentFloor = 0, PassengerCount = 0 };

        //------------------------- Act ----------------------------
        var elevator = CreateStandardElevator(id: 1, maxCapacity: 8);

        //------------------------- Assert --------------------------
        elevator.MaxCapacity.Should().Be(8);
        elevator.PendingRequests.Should().HaveCount(0);
        elevator.Status.Should().BeEquivalentTo(expectedStatus);
    }

    [TestCase(3)]
    [TestCase(4)]
    public void HasCapacity_Given_Elevator_Has_Space_Should_Return_True(int currentPassengers)
    {
        //-------------------------------- Arrange --------------------------------
        var elevator = CreateStandardElevator(id: 1, maxCapacity: 5);

        elevator.Status.PassengerCount = currentPassengers;

        //-------------------------------- Act ------------------------------------
        var actual = elevator.HasCapacity();

        //-------------------------------- Assert ---------------------------------
        actual.Should().BeTrue();
        elevator.MaxCapacity.Should().Be(5);
        elevator.PendingRequests.Should().HaveCount(0);
    }

    [TestCase(5)]
    [TestCase(6)]
    public void HasCapacity_Given_Elevator_Has_No_Space_Should_Return_False(int currentPassengers)
    {
        //-------------------------------- Arrange --------------------------------
        var elevator = CreateStandardElevator(id: 1, maxCapacity: 5);

        elevator.Status.PassengerCount = currentPassengers;

        //-------------------------------- Act ------------------------------------
        var actual = elevator.HasCapacity();

        //-------------------------------- Assert ---------------------------------
        actual.Should().BeFalse();
        elevator.MaxCapacity.Should().Be(5);
        elevator.PendingRequests.Should().HaveCount(0);
    }

    [Test]
    public void AddRequest_Should_Merge_Same_Pickup_And_Destination()
    {
        //-------------------------------- Arrange --------------------------------
        var elevator = CreateStandardElevator(id: 1, maxCapacity: 5);

        elevator.AddRequest(pickupFloor: 1, destinationFloor: 5, waitingPassengers: 2);
        elevator.AddRequest(pickupFloor: 1, destinationFloor: 5, waitingPassengers: 3);

        //-------------------------------- Act ------------------------------------
        elevator.Move(); // goes to floor 1

        //-------------------------------- Assert ---------------------------------
        elevator.Status.CurrentFloor.Should().Be(1);
        elevator.Status.PassengerCount.Should().Be(5);
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
        elevator.Status.PassengerCount.Should().Be(0);
        elevator.PendingRequests.Should().HaveCount(0);
        elevator.Status.Direction.Should().Be(Direction.Idle);
    }

    [Test]
    public void Move_Given_Correct_Request_Should_Pickup_And_DropOff_Accurately()
    {
        //-------------------------------- Arrange --------------------------------
        var elevator = CreateStandardElevator(id: 1, startFloor: 0, maxCapacity: 4);

        elevator.AddRequest(pickupFloor: 1, destinationFloor: 5, waitingPassengers: 3);
        elevator.AddRequest(pickupFloor: 2, destinationFloor: 4, waitingPassengers: 3);

        //-------------------------------- Act ------------------------------------
        elevator.Move(); // should go to floor 1

        //-------------------------------- Assert ---------------------------------
        elevator.HasCapacity().Should().BeTrue();
        elevator.Status.CurrentFloor.Should().Be(1);
        elevator.Status.PassengerCount.Should().Be(3);

        elevator.Move(); // should go to floor 2

        //-------------------------------- Assert ---------------------------------
        elevator.HasCapacity().Should().BeFalse();
        elevator.Status.CurrentFloor.Should().Be(2);
        elevator.Status.PassengerCount.Should().Be(4); // take one passengers and gets fully occupied

        elevator.Move(); // should go to floor 3

        //-------------------------------- Assert ---------------------------------
        elevator.HasCapacity().Should().BeFalse();
        elevator.Status.CurrentFloor.Should().Be(3);
        elevator.Status.PassengerCount.Should().Be(4);

        elevator.Move(); // should move to floor 4 and drop off 1 passenger

        //-------------------------------- Assert ---------------------------------
        elevator.HasCapacity().Should().BeTrue();
        elevator.Status.CurrentFloor.Should().Be(4);
        elevator.Status.PassengerCount.Should().Be(3);

        elevator.Move(); // reaches the last destination and drop off 3 passenger3

        //-------------------------------- Assert ---------------------------------
        elevator.HasCapacity().Should().BeTrue();
        elevator.Status.CurrentFloor.Should().Be(5);
        elevator.Status.IsMoving.Should().BeFalse();
        elevator.Status.PassengerCount.Should().Be(0);
    }

    [Test]
    public void Move_Given_Request_On_Current_Floor_Should_Immediately_Stop()
    {
        //-------------------------------- Arrange --------------------------------
        var elevatorId = 1;
        var elevator = CreateStandardElevator(elevatorId, startFloor: 2);

        elevator.AddRequest(pickupFloor: 2, waitingPassengers: 2, destinationFloor: 2);

        //-------------------------------- Act ------------------------------------
        elevator.Move();

        //-------------------------------- Assert ---------------------------------
        elevator.Status.CurrentFloor.Should().Be(2);
        elevator.Status.IsMoving.Should().BeFalse();
        elevator.Status.PassengerCount.Should().Be(2);
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