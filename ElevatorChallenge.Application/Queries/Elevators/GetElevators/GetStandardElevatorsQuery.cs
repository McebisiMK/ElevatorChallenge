using ElevatorChallenge.Application.Common.Interfaces;
using MediatR;

namespace ElevatorChallenge.Application.Queries.Elevators.GetElevators
{
    public class GetStandardElevatorsQuery : IRequest<IList<IElevator>>
    {
        public int Count { get; set; }

        public class GetStandardElevatorsQueryHandler(IElevatorService elevatorService) : IRequestHandler<GetStandardElevatorsQuery, IList<IElevator>>
        {
            private readonly IElevatorService elevatorService = elevatorService;

            public async Task<IList<IElevator>> Handle(GetStandardElevatorsQuery request, CancellationToken cancellationToken)
            {
                return elevatorService.GetStandardElevators(request.Count);
            }
        }
    }
}
