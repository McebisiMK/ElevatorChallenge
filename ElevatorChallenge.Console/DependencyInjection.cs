using ElevatorChallenge.Application;
using ElevatorChallenge.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ElevatorChallenge.Console;

public static class DependencyInjection
{
    public static IServiceProvider RegisterServices()
    {
        var configuration = new ConfigurationBuilder()
                                 .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)!.FullName)
                                 .AddJsonFile("appsettings.json", optional: false)
                                 .Build();

        var services = new ServiceCollection();
        services.RegisterApplication();
        services.RegisterInfrastructure();
        services.AddSingleton<IConfiguration>(configuration);

        return services.BuildServiceProvider();
    }
}
