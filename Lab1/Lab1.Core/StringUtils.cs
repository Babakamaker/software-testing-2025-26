namespace Lab1.Core;
public static class StringUtils
{
    public static string Capitalize(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var words = input.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++)
        {
            words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
        }

        return string.Join(" ", words);
    }

    public static string Reverse(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return new string(input.Reverse().ToArray());
    }

    public static bool IsPalindrome(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var normalized = input.ToLower();

        var reversed = new string(normalized.Reverse().ToArray());

        return normalized == reversed;
    }

    public static string Truncate(string input, int maxLength)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (input.Length <= maxLength)
            return input;

        return input.Substring(0, maxLength) + "...";
    }
}