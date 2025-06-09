using ElevatorChallenge.Domain.Entities;

namespace ElevatorChallenge.Application.Common.Interfaces;

public interface IElevator
{
    int MaxCapacity { get; }
    ElevatorStatus Status { get; }
    IReadOnlyCollection<int> PendingRequests { get; }

    void Move();
    bool HasCapacity();
    void AddRequest(int pickupFloor, int destinationFloor, int waitingPassengers);
}