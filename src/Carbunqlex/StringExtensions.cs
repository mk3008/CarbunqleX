using System.Globalization;

namespace Carbunqlex;

public static class StringExtensions
{
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return string.Join("", input
            .Split('_')
            .Select(word => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word.ToLower())));
    }
}
