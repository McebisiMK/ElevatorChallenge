using ElevatorChallenge.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ElevatorChallenge.Infrastructure.Services;

public class ElevatorService(IConfiguration configuration) : IElevatorService
{
    public IList<IElevator> GetStandardElevators(int count)
    {
        return [.. Enumerable.Range(1, count).Select(id => new StandardElevator(configuration, id)).Cast<IElevator>()];
    }
}