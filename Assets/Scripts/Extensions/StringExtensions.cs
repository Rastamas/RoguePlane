using System.Text.RegularExpressions;

public static class StringExtensions
{
    public static string SplitCamelCase(this string source) => string.Join(" ", Regex.Split(source, @"(?<!^)(?=[A-Z](?![A-Z]|$))"));
}
