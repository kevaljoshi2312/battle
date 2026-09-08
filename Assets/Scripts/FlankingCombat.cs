using UnityEngine;

public enum FlankType
{
    Front,
    Side,
    Back
}

public static class FlankingCombat
{
    public static FlankType GetFlankType(Vector3 attackerPosition, Health defender)
    {
        if (defender == null)
            return FlankType.Front;

        Vector3 forward = defender.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            return FlankType.Front;

        forward.Normalize();

        Vector3 toAttacker = attackerPosition - defender.transform.position;
        toAttacker.y = 0f;
        if (toAttacker.sqrMagnitude < 0.001f)
            return FlankType.Front;

        toAttacker.Normalize();
        float dot = Vector3.Dot(forward, toAttacker);

        if (dot >= UnitVisuals.FlankFrontDotThreshold)
            return FlankType.Front;

        if (dot <= UnitVisuals.FlankBackDotThreshold)
            return FlankType.Back;

        return FlankType.Side;
    }

    public static float GetDamageMultiplier(FlankType flankType)
    {
        return flankType switch
        {
            FlankType.Side => UnitVisuals.FlankSideDamageMultiplier,
            FlankType.Back => UnitVisuals.FlankBackDamageMultiplier,
            _ => 1f
        };
    }

    public static int ApplyFlankingDamage(int baseDamage, Vector3 attackerPosition, Health target, out FlankType flankType)
    {
        flankType = GetFlankType(attackerPosition, target);
        float multiplier = GetDamageMultiplier(flankType);
        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
    }

    public static string GetFlankLabel(FlankType flankType)
    {
        return flankType switch
        {
            FlankType.Back => "BACK HIT +50%",
            FlankType.Side => "FLANK +25%",
            _ => string.Empty
        };
    }
}
