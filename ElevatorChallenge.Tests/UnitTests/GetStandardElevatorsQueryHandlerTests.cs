using ElevatorChallenge.Application.Common.Interfaces;
using ElevatorChallenge.Application.Queries.Elevators.GetElevators;
using FluentAssertions;
using NSubstitute;
using static ElevatorChallenge.Application.Queries.Elevators.GetElevators.GetStandardElevatorsQuery;

namespace ElevatorChallenge.Tests.UnitTests;

[TestFixture]
public class GetStandardElevatorsQueryHandlerTests
{
    [Test]
    public void Validate_Given_Count_Equal_To_Zero_Should_Fail_And_Return_Correct_Validation_Error()
    {
        //------------------------------ Arrange ------------------------------
        var expectedErrors = new List<string> { "Number of requested elevator creation must be greater than 0." };
        var query = new GetStandardElevatorsQuery { Count = 0 };
        var validator = new GetStandardElevatorsQueryValidator();

        //------------------------------ Act ----------------------------------
        var actual = validator.Validate(query);

        //------------------------------ Assert -------------------------------
        actual.IsValid.Should().BeFalse();
        actual.Errors.Should().ContainSingle(e => e.PropertyName == "Count");
        actual.Errors.Select(x => x.ErrorMessage).Should().BeEquivalentTo(expectedErrors);
    }

    [TestCase(1)]
    [TestCase(3)]
    [TestCase(5)]
    public async Task Handle_Given_Number_Of_Elevators_To_Create_Should_Return_Correct_Number_Of_Elevators(int elevatorsCount)
    {
        //-------------------------------------- Arrange --------------------------------------
        List<IElevator> dummyElevators = [.. Enumerable.Range(1, elevatorsCount).Select(x => Substitute.For<IElevator>())];
        var elevatorService = Substitute.For<IElevatorService>();
        var standardElevatorQuery = CreateStandardElevatorQuery(elevatorsCount);
        var standardElevatorQueryHandler = CreateStandardElevatorsQueryHandler(elevatorService);

        elevatorService.GetStandardElevators(elevatorsCount).Returns(dummyElevators);

        //-------------------------------------- Act ------------------------------------------
        var actual = await standardElevatorQueryHandler.Handle(standardElevatorQuery, CancellationToken.None);

        //-------------------------------------- Assert ---------------------------------------
        actual.Should().HaveCount(elevatorsCount);
        elevatorService.Received(1).GetStandardElevators(elevatorsCount);
    }

    private static GetStandardElevatorsQueryHandler CreateStandardElevatorsQueryHandler(IElevatorService elevatorService)
    {
        return new GetStandardElevatorsQueryHandler(elevatorService);
    }

    private static GetStandardElevatorsQuery CreateStandardElevatorQuery(int count)
    {
        return new GetStandardElevatorsQuery { Count = count };
    }
}