namespace ElevatorChallenge.Application.Common.Interfaces;

public interface IElevatorService
{
    IList<IElevator> GetStandardElevators(int count);
}