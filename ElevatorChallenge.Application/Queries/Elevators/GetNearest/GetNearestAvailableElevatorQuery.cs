using ElevatorChallenge.Application.Common.Interfaces;
using ElevatorChallenge.Application.Queries.Elevators.GetElevators;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ElevatorChallenge.Application.Queries.Elevators.GetNearest;

public class GetNearestAvailableElevatorQuery : IRequest<IElevator?>
{
    public int Floor { get; set; }

    public class GetNearestAvailableElevatorQueryHandler(IMediator mediator, IConfiguration configuration) : IRequestHandler<GetNearestAvailableElevatorQuery, IElevator?>
    {
        private readonly IMediator mediator = mediator;
        private readonly IConfiguration configuration = configuration;

        public async Task<IElevator?> Handle(GetNearestAvailableElevatorQuery request, CancellationToken cancellationToken)
        {
            var numberOfElevators = configuration.GetValue<int>("ElevatorOptions:ElevatorDefaultMaxCapacity");
            var elevators = await mediator.Send(new GetStandardElevatorsQuery { Count = numberOfElevators }, cancellationToken: cancellationToken);

            return elevators
                    .Where(elevator => elevator.HasCapacity())
                    .OrderBy(elevator => Math.Abs(elevator.Status.CurrentFloor - request.Floor))
                    .FirstOrDefault();
        }
    }
}