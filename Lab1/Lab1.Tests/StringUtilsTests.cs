using Lab1.Core;
using Shouldly;
using Xunit;

namespace Lab1.Tests;
public class StringUtilsTests
{
    // Capitalize
    [Theory]
    [InlineData("hello world", "Hello World")]
    [InlineData("HELLO", "Hello")]
    [InlineData("a", "A")]
    public void Capitalize_ValidInput_ReturnsCapitalized(string input, string expected)
    {
        StringUtils.Capitalize(input).ShouldBe(expected);
    }

    [Fact]
    public void Capitalize_Null_ThrowsException()
    {
        Should.Throw<ArgumentNullException>(() => StringUtils.Capitalize(null));
    }

    // Reverse
    [Theory]
    [InlineData("abcd", "dcba")]
    [InlineData("a", "a")]
    [InlineData("hello", "olleh")]
    public void Reverse_ValidInput_ReturnsReversed(string input, string expected)
    {
        StringUtils.Reverse(input).ShouldBe(expected);
    }

    [Fact]
    public void Reverse_Null_ThrowsException()
    {
        Should.Throw<ArgumentNullException>(() => StringUtils.Reverse(null));
    }

    // IsPalindrome
    [Theory]
    [InlineData("Racecar", true)]
    [InlineData("Hello", false)]
    [InlineData("a", true)]
    public void IsPalindrome_ChecksCorrectly(string input, bool expected)
    {
        StringUtils.IsPalindrome(input).ShouldBe(expected);
    }

    [Fact]
    public void IsPalindrome_Null_ThrowsException()
    {
        Should.Throw<ArgumentNullException>(() => StringUtils.IsPalindrome(null));
    }

    // Truncate
    [Theory]
    [InlineData("Hello World", 5, "Hello...")]
    [InlineData("Hi", 10, "Hi")]
    [InlineData("A", 1, "A")]
    public void Truncate_ValidInput_ReturnsExpected(string input, int maxLength, string expected)
    {
        StringUtils.Truncate(input, maxLength).ShouldBe(expected);
    }

    [Fact]
    public void Truncate_Null_ThrowsException()
    {
        Should.Throw<ArgumentNullException>(() => StringUtils.Truncate(null, 5));
    }
}