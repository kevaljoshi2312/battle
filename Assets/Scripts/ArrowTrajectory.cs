using UnityEngine;

public static class ArrowTrajectory
{
    public static float GetArcHeight(float horizontalDistance)
    {
        return Mathf.Clamp(
            horizontalDistance * UnitVisuals.ArrowArcHeightScale,
            UnitVisuals.ArrowArcMinHeight,
            UnitVisuals.ArrowArcMaxHeight);
    }

    public static float GetFlightDuration(float horizontalDistance, float speed)
    {
        return horizontalDistance / Mathf.Max(speed, 0.01f);
    }

    public static Vector3 GetPoint(Vector3 start, Vector3 end, float t, float arcHeight)
    {
        t = Mathf.Clamp01(t);
        Vector3 point = Vector3.Lerp(start, end, t);
        point.y = Mathf.Lerp(start.y, end.y, t) + arcHeight * 4f * t * (1f - t);
        return point;
    }

    public static Vector3 GetTangent(Vector3 start, Vector3 end, float t, float arcHeight)
    {
        const float sampleDelta = 0.02f;
        float t0 = Mathf.Clamp01(t - sampleDelta);
        float t1 = Mathf.Clamp01(t + sampleDelta);
        if (Mathf.Approximately(t0, t1))
            t1 = Mathf.Min(1f, t0 + sampleDelta);

        Vector3 p0 = GetPoint(start, end, t0, arcHeight);
        Vector3 p1 = GetPoint(start, end, t1, arcHeight);
        Vector3 tangent = p1 - p0;
        if (tangent.sqrMagnitude < 0.0001f)
            tangent = end - start;

        return tangent.normalized;
    }

    public static float GetHorizontalDistance(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        delta.y = 0f;
        return delta.magnitude;
    }
}
