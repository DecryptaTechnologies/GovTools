namespace DecryptaTechnologies.GovTools.WpfUI.Utils;

public static class StringExtensions
{

    /// <summary>
    /// Get substring of specified number of characters on the right.
    /// </summary>
    public static string Right(this string value, int length)
    {
        if (!string.IsNullOrEmpty(value))
            return value.Length <= length ? value : value.Substring(value.Length - length);
        return "";
    }

}
