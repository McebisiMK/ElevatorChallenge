using ElevatorChallenge.Application.Queries.Elevators.GetElevators;
using ElevatorChallenge.Application.Queries.Elevators.GetNearest;
using ElevatorChallenge.Console;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = DependencyInjection.RegisterServices();
var mediator = serviceProvider.GetRequiredService<IMediator>();
var configuration = serviceProvider.GetRequiredService<IConfiguration>();

var numberOfElevators = configuration.GetValue<int>("ElevatorOptions:NumberOfStandardElevators");
var elevators = await mediator.Send(new GetStandardElevatorsQuery { Count = numberOfElevators });

while (true)
{
    try
    {
        Console.WriteLine("================================== ELEVATOR STATUS ==================================");

        foreach (var elevator in elevators)
        {
            var status = elevator.Status;

            Console.WriteLine($"Elevator: {status.Id} | Floor: {status.CurrentFloor} ({(status.IsMoving ? status.Direction.ToString() : "-")}) | Passengers: {status.PassengerCount}");
        }

        Console.WriteLine();
        Console.WriteLine("Please enter request floor: ");

        if (!int.TryParse(Console.ReadLine(), out int pickUpFloor))
        {
            Console.WriteLine("Floor must be a number");
            Thread.Sleep(2000);
            continue;
        }

        Console.WriteLine("Please enter number of waiting passengers (greater than 0): ");

        if (!int.TryParse(Console.ReadLine(), out int waitingPassengers))
        {
            Console.WriteLine("Floor must be a number");
            Thread.Sleep(2000);
            continue;
        }

        Console.Write("Enter destination floor: ");

        if (!int.TryParse(Console.ReadLine(), out int destinationFloor) || destinationFloor == pickUpFloor)
        {
            Console.WriteLine("Destination must be a valid number and different from pickup floor.");
            Thread.Sleep(2000);
            continue;
        }

        var nearestElevator = await mediator.Send(new GetNearestAvailableElevatorQuery { Floor = pickUpFloor, Elevators = elevators });

        if (nearestElevator is not null)
        {
            nearestElevator.AddRequest(pickUpFloor, destinationFloor, waitingPassengers);

            Console.WriteLine($"Elevator {nearestElevator.Status.Id} dispatched to floor {pickUpFloor}.");
        }
        else
        {
            Console.WriteLine("No elevator with available capacity at the moment.");
        }

        Console.WriteLine();

        foreach (var elevator in elevators)
        {
            elevator.Move();
        }

        Thread.Sleep(3000);
    }
    catch (ValidationException ex)
    {
        foreach (var failure in ex.Errors)
        {
            Console.WriteLine($"Validation Error: {failure.ErrorMessage}");
        }
        Thread.Sleep(3000);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected Error: {ex.Message}");
        Thread.Sleep(3000);
    }
}