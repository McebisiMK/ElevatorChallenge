using ElevatorChallenge.Application.Common.Interfaces;
using ElevatorChallenge.Domain.Entities;
using ElevatorChallenge.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace ElevatorChallenge.Infrastructure.Services;

public class StandardElevator : IElevator
{
    private readonly Queue<int> requests = new();

    public int MaxCapacity { get; }
    public ElevatorStatus Status { get; private set; }
    public IReadOnlyCollection<int> PendingRequests => [.. requests];

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

    public void AddRequest(int floor)
    {
        if (!requests.Contains(floor)) requests.Enqueue(floor);
    }

    public void Move()
    {
        if (NoOutstandingRequests())
        {
            UpdateMovement(Direction.Idle, false);
            return;
        }

        var nextFloor = requests.Peek();
        var direction = GetDirection(nextFloor);

        UpdateMovement(direction, true);
        UpdatePosition(nextFloor);

        if (IsCurrentLevelRequest(nextFloor)) UpdateStatusOnArrival();
    }

    private void UpdateStatusOnArrival()
    {
        requests.Dequeue();

        if (NoOutstandingRequests())
        {
            UpdateMovement(Direction.Idle, false);
            return;
        }

        Status.IsMoving = false;
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

    private bool NoOutstandingRequests() => requests.Count == 0;

    private bool IsCurrentLevelRequest(int nextFloor) => Status.CurrentFloor == nextFloor;

    private bool IsUpperLevelRequest(int nextFloor) => nextFloor > Status.CurrentFloor;

    private bool IsLowerLevelRequest(int nextFloor) => nextFloor < Status.CurrentFloor;
}
