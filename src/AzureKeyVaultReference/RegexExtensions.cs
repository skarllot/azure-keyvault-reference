using System.Text.RegularExpressions;

namespace Raiqub.AzureKeyVaultReference;

internal static class RegexExtensions
{
    public static string? MatchAndGetGroupValue(this Regex regex, string input, string groupName)
    {
        var match = regex.Match(input);
        return match.Success ? match.Groups[groupName].Value : null;
    }
}
