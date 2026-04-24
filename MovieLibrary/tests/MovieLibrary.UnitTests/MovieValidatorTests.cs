using MovieLibrary.Api.Services;
using Shouldly;

namespace MovieLibrary.UnitTests;

public class MovieValidatorTests
{
    [Fact]
    public void ValidateReleaseYear_FutureYear_ReturnsValidationMessage()
    {
        var sut = new MovieValidator();

        var result = sut.ValidateReleaseYear(DateTime.UtcNow.Year + 1);

        result.ShouldBe("Release year cannot be in the future.");
    }

    [Fact]
    public void ValidateReleaseYear_CurrentYear_ReturnsNull()
    {
        var sut = new MovieValidator();

        var result = sut.ValidateReleaseYear(DateTime.UtcNow.Year);

        result.ShouldBeNull();
    }
}
