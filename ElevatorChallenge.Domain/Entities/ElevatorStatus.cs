using ElevatorChallenge.Domain.Enums;

namespace ElevatorChallenge.Domain.Entities;

public class ElevatorStatus
{
    public int Id { get; set; }
    public bool IsMoving { get; set; }
    public int CurrentFloor { get; set; }
    public int PassengerCount { get; set; }
    public Direction Direction { get; set; }
}
