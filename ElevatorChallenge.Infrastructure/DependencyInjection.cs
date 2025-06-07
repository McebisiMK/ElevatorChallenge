using ElevatorChallenge.Application.Common.Interfaces;
using ElevatorChallenge.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ElevatorChallenge.Infrastructure;

public static class DependencyInjection
{
    public static void RegisterInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IElevator, StandardElevator>();
        services.AddTransient<IElevatorService, ElevatorService>();
    }
}
