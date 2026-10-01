using UnityEngine;

/// <summary>The curved flight paths used by the wave entry and the dive bombers.</summary>
public static class Bezier
{
    /// <summary>A point on the quadratic curve from start to end that bends towards control.</summary>
    public static Vector2 Quadratic(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        var remaining = 1f - t;
        return remaining * remaining * start + 2f * remaining * t * control + t * t * end;
    }
}
