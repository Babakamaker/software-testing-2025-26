using Microsoft.EntityFrameworkCore;
using MovieLibrary.Api.Data;
using MovieLibrary.Api.Domain;
using MovieLibrary.Api.Repositories;
using MovieLibrary.Api.Services;
using Shouldly;

namespace MovieLibrary.UnitTests;

public class ReviewRulesValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void ValidateScore_OutOfRangeScore_ReturnsValidationMessage(int score)
    {
        var sut = new ReviewRulesValidator();

        var result = sut.ValidateScore(score);

        result.ShouldBe("Score must be between 1 and 10.");
    }

    [Theory]
    [InlineData(1, "too short")]
    [InlineData(3, "small comment")]
    public void ValidateComment_ShortComment_ReturnsValidationMessage(int score, string comment)
    {
        var sut = new ReviewRulesValidator();

        var result = sut.ValidateComment(score, comment);

        result.ShouldBe("Review must include at least 20 characters in the comment.");
    }

    [Fact]
    public async Task ValidateDuplicateReviewAsync_ExistingReview_ReturnsValidationMessage()
    {
        var options = new DbContextOptionsBuilder<MovieLibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var dbContext = new MovieLibraryDbContext(options);
        var repository = new ReviewRepository(dbContext);
        dbContext.Reviews.Add(new Review
        {
            MovieId = 15,
            UserId = 42,
            Score = 8,
            Comment = "This is a sufficiently detailed review comment.",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await dbContext.SaveChangesAsync();

        var sut = new ReviewRulesValidator();

        var result = await sut.ValidateDuplicateReviewAsync(repository, 15, 42, TestContext.Current.CancellationToken);

        result.ShouldBe("A user can leave only one review per movie.");
    }
}
