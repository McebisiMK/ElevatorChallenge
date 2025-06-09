using FluentValidation;

namespace ElevatorChallenge.Application.Queries.Elevators.GetNearest;

public class GetNearestAvailableElevatorQueryValidator : AbstractValidator<GetNearestAvailableElevatorQuery>
{
    public GetNearestAvailableElevatorQueryValidator()
    {
        RuleFor(request => request.Floor)
         .GreaterThan(0)
         .WithMessage("Floor number must be greater than 0.");

        RuleFor(request => request.Elevators)
         .NotEmpty()
         .WithMessage("Please supply list of elevators");
    }
}