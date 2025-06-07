using ElevatorChallenge.Domain.Entities;

namespace ElevatorChallenge.Application.Common.Interfaces;

public interface IElevator
{
    int MaxCapacity { get; }
    ElevatorStatus Status { get; }

    void Move();
    void AddRequest(int floor);
    bool CanAcceptPassenger(int count);
}