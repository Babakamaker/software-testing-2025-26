using Lab1.Core;
using Shouldly;
using Xunit;

namespace Lab1.Tests;

public class CalculatorTests
{
    private readonly Calculator<int> _intCalc = new();
    private readonly Calculator<double> _doubleCalc = new();

    // Add
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsCorrectSum()
    {
        int a = 4, b = 3;

        var result = _intCalc.Add(a, b);

        result.ShouldBe(7);
    }

    [Fact]
    public void Add_TwoNegativeNumbers_ReturnsCorrectSum()
    {
        var result = _intCalc.Add(-4, -6);

        result.ShouldBe(-10);
    }

    [Fact]
    public void Add_DoubleNumbers_ReturnsCorrectSum()
    {
        var result = _doubleCalc.Add(4.5, 3.0);

        result.ShouldBe(7.5);
    }

    // Subtract
    [Fact]
    public void Subtract_TwoPositiveNumbers_ReturnsCorrectDifference()
    {
        var result = _intCalc.Subtract(10, 4);

        result.ShouldBe(6);
    }

    [Fact]
    public void Subtract_WithNegativeNumber_ReturnsCorrectDifference()
    {
        var result = _intCalc.Subtract(5, -3);

        result.ShouldBe(8);
    }

    [Fact]
    public void Subtract_DoubleNumbers_ReturnsCorrectDifference()
    {
        var result = _doubleCalc.Subtract(10.0, 4.3);

        result.ShouldBe(5.7);
    }

    // Multiply
    [Fact]
    public void Multiply_TwoPositiveNumbers_ReturnsCorrectProduct()
    {
        var result = _intCalc.Multiply(3, 4);

        result.ShouldBe(12);
    }

    [Fact]
    public void Multiply_WithNegativeNumber_ReturnsCorrectProduct()
    {
        var result = _intCalc.Multiply(-3, 5);

        result.ShouldBe(-15);
    }

    // Divide

    [Theory]
    [InlineData(10, 5, 2)]
    [InlineData(-10, 2, -5)]
    public void Divide_ValidInputs_ReturnsQuotient(double a, double b, double expected)
    {
        _doubleCalc.Divide(a, b).ShouldBe(expected);
    }

    [Fact]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        Should.Throw<DivideByZeroException>(() => _intCalc.Divide(10, 0));
    }

    // Float

    [Fact]
    public void Add_FloatingPointNumbers_ReturnsApproximateResult()
    {
        var result = _doubleCalc.Add(0.1, 0.2);

        result.ShouldBe(0.3, 0.0001);
    }
}

