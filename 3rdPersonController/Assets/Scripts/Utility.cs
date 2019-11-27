using UnityEngine;

/// <summary>
/// General custom math functions.
/// </summary>
public class Utility
{
    /// <summary>
    /// Gets the percentage of the value between the min and max values.
    /// </summary>
    /// <param name="value">The value that is being converted.</param>
    /// <param name="min">Minimum value (0%).</param>
    /// <param name="max">Maximum value (100%).</param>
    /// <returns>The value as a percentage.</returns>
    public static float ProgressAsPercentage(float value, float min, float max)
    {
        // clamps the value between the min and max
        if (value <= min)
        {
            return 0.0f;
        }
        else if (value >= max)
        {
            return 1.0f;
        }

        // gets the value and the maximum relative to the minimum
        float relativeValue = value - min;
        float relativeMax = max - min;

        // returns the value as a percentage
        return relativeValue / relativeMax;
    }

    /// <summary>
    /// Gets the value between the min and max from a percentage.
    /// </summary>
    /// <param name="value">The percentage.</param>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <returns>A value at the percentage.</returns>
    public static float ProgressAsDecimal(float value, float min, float max)
    {
        // gets the maximum relative to the minimum
        float relativeMax = max - min;
        // works out the value relative to the minimum
        float relativeValue = relativeMax * value;

        // returns the value relative to the world.
        return relativeValue + min;
    }

    /// <summary>
    /// Gets a colour with the specified alpha.
    /// </summary>
    /// <param name="colour">The colour that is having its alpha changed.</param>
    /// <param name="alpha">The new alpha.</param>
    /// <returns></returns>
    public static Color ChangeAlpha(Color colour, float alpha)
    {
        return new Color(colour.r, colour.g, colour.b, alpha);
    }
}