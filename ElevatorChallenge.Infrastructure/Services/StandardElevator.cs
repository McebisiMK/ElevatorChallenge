using ElevatorChallenge.Application.Common.Interfaces;
using ElevatorChallenge.Domain.Entities;
using ElevatorChallenge.Domain.Enums;
using ElevatorChallenge.Infrastructure.Models;
using Microsoft.Extensions.Configuration;

namespace ElevatorChallenge.Infrastructure.Services;

public class StandardElevator : IElevator
{
    private readonly List<Destination> destinations = [];
    private readonly Queue<(int PickUpFloor, int DestinationFloor, int Passengers)> pickUpQueue = new();

    public int MaxCapacity { get; }
    public ElevatorStatus Status { get; private set; }
    public IReadOnlyCollection<int> PendingRequests => [.. pickUpQueue.Select(x => x.PickUpFloor)];

    public StandardElevator(IConfiguration configuration, int id, int startFloor = 0)
    {
        Status = new ElevatorStatus
        {
            Id = id,
            IsMoving = false,
            PassengerCount = 0,
            CurrentFloor = startFloor,
            Direction = Direction.Idle,
        };
        MaxCapacity = configuration.GetValue<int>("ElevatorOptions:ElevatorDefaultMaxCapacity");
    }

    public bool HasCapacity() => Status.PassengerCount < MaxCapacity;

    public void AddRequest(int pickupFloor, int destinationFloor, int waitingPassengers)
    {
        var requests = pickUpQueue.ToList();
        var index = requests.FindIndex(q => q.PickUpFloor == pickupFloor && q.DestinationFloor == destinationFloor);

        if (index != -1)
        {
            var (floor, destination, passeners) = requests[index];
            requests[index] = (floor, destination, passeners + waitingPassengers);
        }
        else
        {
            requests.Add((pickupFloor, destinationFloor, waitingPassengers));
        }

        pickUpQueue.Clear();
        requests.ForEach(request => pickUpQueue.Enqueue(request));
    }

    public void Move()
    {
        if (!HasPendingStops())
        {
            UpdateMovement(Direction.Idle, false);
            return;
        }

        var targetFloor = GetNextStopFloor();
        var direction = GetDirection(targetFloor);

        UpdateMovement(direction, true);
        UpdatePosition(targetFloor);

        if (IsCurrentLevelRequest(targetFloor))
        {
            UpdateStatusOnArrival(targetFloor);
            Status.PassengerCount = destinations.Count;
        }
    }

    private void UpdateStatusOnArrival(int targetFloor)
    {
        DropOffPassengers();

        if (IsPickUpFloor(targetFloor))
        {
            var (floor, destionation, passengers) = pickUpQueue.Dequeue();
            var availableSpace = MaxCapacity - destinations.Count;
            var acceptablePassengers = Math.Min(availableSpace, passengers);

            for (int i = 0; i < acceptablePassengers; i++)
            {
                destinations.Add(new Destination { Floor = destionation });
            }
        }

        if (!HasPendingStops())
        {
            UpdateMovement(Direction.Idle, false);
        }
        else
        {
            Status.IsMoving = false;
        }
    }

    private void DropOffPassengers()
    {
        var dropOffPassengers = destinations.Where(d => d.Floor == Status.CurrentFloor).ToList();

        dropOffPassengers.ForEach(passenger => destinations.Remove(passenger));
    }

    private int GetNextStopFloor()
    {
        var currentFloor = Status.CurrentFloor;
        var allStops = pickUpQueue.Select(p => p.PickUpFloor).Concat(destinations.Select(d => d.Floor)).Distinct().ToList();

        return allStops
            .OrderBy(floor => Math.Abs(floor - currentFloor))
            .FirstOrDefault();
    }

    private void UpdateMovement(Direction direction, bool isMoving)
    {
        Status.IsMoving = isMoving;
        Status.Direction = direction;
    }

    private void UpdatePosition(int nextFloor)
    {
        if (IsUpperLevelRequest(nextFloor)) Status.CurrentFloor++;

        if (IsLowerLevelRequest(nextFloor)) Status.CurrentFloor--;
    }

    private Direction GetDirection(int nextFloor)
    {
        return IsUpperLevelRequest(nextFloor) ? Direction.Up : Direction.Down;
    }

    private bool IsUpperLevelRequest(int nextFloor) => nextFloor > Status.CurrentFloor;

    private bool IsLowerLevelRequest(int nextFloor) => nextFloor < Status.CurrentFloor;

    private bool IsCurrentLevelRequest(int nextFloor) => Status.CurrentFloor == nextFloor;

    private bool HasPendingStops() => pickUpQueue.Any() || destinations.Any(d => d.Floor != Status.CurrentFloor);

    private bool IsPickUpFloor(int targetFloor) => pickUpQueue.Any() && pickUpQueue.Peek().PickUpFloor == targetFloor;
}
