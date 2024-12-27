using System.Globalization;

public static class DoubleExtensions
{
    static string ToParsableString(this double x)
    {
        return x.ToString("G17", CultureInfo.InvariantCulture);
    }
}