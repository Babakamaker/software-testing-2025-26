using MovieLibrary.Api.Services;
using Shouldly;

namespace MovieLibrary.UnitTests;

public class RatingCalculatorTests
{
    [Fact]
    public void CalculateAverageRating_ThreeScores_ReturnsRoundedAverage()
    {
        var sut = new RatingCalculator();

        var result = sut.CalculateAverageRating([8, 9, 10]);

        result.ShouldBe(9m);
    }

    [Fact]
    public void CalculateAverageRating_NoScores_ReturnsZero()
    {
        var sut = new RatingCalculator();

        var result = sut.CalculateAverageRating([]);

        result.ShouldBe(0m);
    }

    [Fact]
    public void RoundAverage_TwoDecimalAverage_ReturnsRoundedValue()
    {
        var sut = new RatingCalculator();

        var result = sut.RoundAverage(8.555m);

        result.ShouldBe(8.56m);
    }
}
