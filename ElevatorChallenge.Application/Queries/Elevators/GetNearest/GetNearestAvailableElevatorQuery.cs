using ElevatorChallenge.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ElevatorChallenge.Application.Queries.Elevators.GetNearest;

public class GetNearestAvailableElevatorQuery : IRequest<IElevator?>
{
    public int Floor { get; set; }
    public IList<IElevator> Elevators { get; set; } = [];

    public class GetNearestAvailableElevatorQueryHandler(IMediator mediator, IConfiguration configuration) : IRequestHandler<GetNearestAvailableElevatorQuery, IElevator?>
    {
        private readonly IMediator mediator = mediator;
        private readonly IConfiguration configuration = configuration;

        public async Task<IElevator?> Handle(GetNearestAvailableElevatorQuery request, CancellationToken cancellationToken)
        {
            var nearestElevator = GetNearestElevator(request.Elevators, request.Floor);

            return nearestElevator;
        }

        private static IElevator? GetNearestElevator(IList<IElevator> elevators, int floor)
        {
            return elevators
                    .Where(elevator => elevator.HasCapacity())
                    .OrderBy(elevator => Math.Abs(elevator.Status.CurrentFloor - floor))
                    .FirstOrDefault();
        }
    }
}