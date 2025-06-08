using FluentValidation;

namespace ElevatorChallenge.Application.Queries.Elevators.GetElevators
{
    public class GetStandardElevatorsQueryValidator : AbstractValidator<GetStandardElevatorsQuery>
    {
        public GetStandardElevatorsQueryValidator()
        {
            RuleFor(request => request.Count)
             .GreaterThan(0)
             .WithMessage("Number of requested elevator creation must be greater than 0.");
        }
    }
}
