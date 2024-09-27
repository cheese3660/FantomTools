namespace FantomTools.Utilities;

/// <summary>
/// Adds methods for handling fantom durations
/// </summary>
public static class DurationExtensions
{
    /// <summary>
    /// Converts a string to a duration
    /// </summary>
    /// <param name="duration">The string to convert</param>
    /// <returns>The string as a duration</returns>
    /// <exception cref="ArgumentException">When the string is an invalid duration</exception>
    public static long ToDurationValue(this string duration)
    {
        var removedUnderscores = duration.Replace("_", "");
        long multiplier = 1;
        if (removedUnderscores.EndsWith("ns"))
        {
            removedUnderscores = removedUnderscores.Replace("ns", "").Trim();
        } else if (removedUnderscores.EndsWith("ms"))
        {
            multiplier = 1_000_000L;
            removedUnderscores = removedUnderscores.Replace("ms", "").Trim();
        } else if (removedUnderscores.EndsWith("sec"))
        {
            multiplier = 1_000_000_000L;
            removedUnderscores = removedUnderscores.Replace("sec", "").Trim();
        } else if (removedUnderscores.EndsWith("min"))
        {
            multiplier = 60_000_000_000L;
            removedUnderscores = removedUnderscores.Replace("min", "").Trim();
        } else if (removedUnderscores.EndsWith("hr"))
        {
            multiplier = 3_600_000_000_000L;
            removedUnderscores = removedUnderscores.Replace("hr", "").Trim();
        } else if (removedUnderscores.EndsWith("day"))
        {
            multiplier = 86_400_000_000_000L;
            removedUnderscores = removedUnderscores.Replace("day", "").Trim();
        }
        else throw new ArgumentException($"Invalid duration {duration}", nameof(duration));
        return long.Parse(removedUnderscores) * multiplier;
    }

    /// <summary>
    /// Converts a long duration value into a duration string
    /// </summary>
    /// <param name="duration">The duration value</param>
    /// <returns>The duration string</returns>
    public static string ToDurationString(this long duration)
    {
        if (duration % 86_400_000_000_000L == 0)
        {
            return $"{duration/86_400_000_000_000L} day";
        }

        if (duration % 3_600_000_000_000L == 0)
        {
            return $"{duration / 3_600_000_000_000L} hr";
        }
        
        if (duration % 60_000_000_000L == 0)
        {
            return $"{duration / 60_000_000_000L} min";
        }
        
        if (duration % 1_000_000_000L == 0)
        {
            return $"{duration / 1_000_000_000L} sec";
        }
        
        if (duration % 1_000_000L == 0)
        {
            return $"{duration / 1_000_000L} ms";
        }

        return $"{duration} ns";
    }
}