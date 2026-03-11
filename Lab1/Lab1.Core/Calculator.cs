using System.Numerics;

namespace Lab1.Core;

public class Calculator<T> where T : INumber<T>
{
    public T Add(T a, T b)
    {
        return a + b;
    }

    public T Subtract(T a, T b)
    {
        return a - b;
    }

    public T Multiply(T a, T b)
    {
        return a * b;
    }

    public T Divide(T a, T b)
    {
        if (b == T.Zero)
            throw new DivideByZeroException();

        return a / b;
    }
}