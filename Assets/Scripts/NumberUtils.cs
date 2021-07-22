using System.Globalization;

internal class NumberUtils
{
    public static string FormatLargeNumbers(double number)
    {
        if (number < 1000)
            return number.ToString("0.#", CultureInfo.InvariantCulture);
        else if (number < 1000000)
            return number.ToString("0,.#K", CultureInfo.InvariantCulture);
        else if (number < 1000000000)
            return number.ToString("0,,.##M", CultureInfo.InvariantCulture);
        else if (number < 1000000000000)
            return number.ToString("0,,,.##B", CultureInfo.InvariantCulture);
        else if (number < 1000000000000000)
            return number.ToString("0,,,,.##T", CultureInfo.InvariantCulture);
        else if (number < 1e+100)
            return number.ToString("0.#e+00", CultureInfo.InvariantCulture);
        else
            return number.ToString("0.##e+000", CultureInfo.InvariantCulture);
    }
}