namespace MovieLibrary.Api.Services;

public class RatingCalculator
{
    public decimal CalculateAverageRating(IEnumerable<int> scores)
    {
        var values = scores.ToArray();
        if (values.Length == 0)
        {
            return 0m;
        }

        var average = values.Select(score => (decimal)score).Average();
        return RoundAverage(average);
    }

    public decimal RoundAverage(decimal? averageScore) =>
        Math.Round(averageScore ?? 0m, 2, MidpointRounding.AwayFromZero);
}
