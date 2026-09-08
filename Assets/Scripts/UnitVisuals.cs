using UnityEngine;

public static class UnitVisuals
{
    public const float CapsuleHeight = 0.8f;
    public const float CapsuleRadius = 0.5f;

    // Gap between units at battle start and when moving as a group.
    public const float SpawnSpacing = 1f;
    public const float GroupMoveSpacing = 1f;

    // Team lines along Z (ground plane center is 0).
    public const float PlayerLineZ = -2f;
    public const float EnemyLineZ = 3f;

    public static Vector3 CapsuleScale => new Vector3(CapsuleRadius, CapsuleHeight, CapsuleRadius);

    // Default capsule primitive radius is 0.5 local; scale.x/z = CapsuleRadius.
    public static float CapsuleWorldRadius => CapsuleRadius * 0.5f;

    public const float HealthBarWidth = 0.5f;
    public const float HealthBarHeight = 0.1f;

    public const float SelectionRingDiameter = 0.65f;
    public const float SelectionRingHeight = 0.04f;

    public const float MeleeAttackRangeMax = 2.5f;
    public const float ArrowSpeed = 16f;
    public const float ArrowHitDistance = 0.35f;
    public const float ArrowSpawnHeight = 0.55f;
    public const float ArrowTargetHeight = 0.45f;
    public const float ArrowArcHeightScale = 0.18f;
    public const float ArrowArcMinHeight = 0.35f;
    public const float ArrowArcMaxHeight = 2.2f;

    public const float ArrowShaftLength = 0.42f;
    public const float ArrowShaftRadius = 0.012f;
    public const float ArrowHeadLength = 0.1f;
    public const float ArrowHeadWidth = 0.045f;
    public const float ArrowFletchLength = 0.08f;
    public const float ArrowFletchWidth = 0.025f;
    public const float ArrowFletchHeight = 0.04f;
    public const float ArrowFletchSpread = 0.028f;
    public const float ArrowTrailDuration = 0.12f;
    public const float ArrowTrailStartWidth = 0.035f;

    public static readonly Color ArrowShaftColor = new Color(0.72f, 0.52f, 0.28f);
    public static readonly Color ArrowHeadColor = new Color(0.72f, 0.74f, 0.78f);
    public static readonly Color ArrowFletchColor = new Color(0.85f, 0.2f, 0.18f);
    public static readonly Color ArrowTrailStartColor = new Color(1f, 0.92f, 0.55f, 0.55f);
    public static readonly Color ArrowTrailEndColor = new Color(1f, 0.85f, 0.35f, 0f);

    // Legacy alias used by older code paths.
    public static readonly Color ArrowColor = ArrowShaftColor;

    public const float FlankSideDamageMultiplier = 1.25f;
    public const float FlankBackDamageMultiplier = 1.5f;
    public const float FlankFrontDotThreshold = 0.45f;
    public const float FlankBackDotThreshold = -0.45f;

    public static bool IsRangedAttack(float attackRange) => attackRange > MeleeAttackRangeMax;

    public const float MeleeChaseStopDistance = 1f;
    public const float RangedChaseStopDistanceInset = 2f;

    public static float GetChaseStopDistance(float attackRange)
    {
        if (IsRangedAttack(attackRange))
            return attackRange - RangedChaseStopDistanceInset;

        return MeleeChaseStopDistance;
    }

    public const float MoveSpeedScale = 0.25f;
    public const float BaseTurnSpeed = 720f;
    public static float TurnSpeed => BaseTurnSpeed * MoveSpeedScale;

    public const float BaseAttackCooldown = 1f;
    public const float AttackCooldownScale = 2f;
    public static float AttackCooldown => BaseAttackCooldown * AttackCooldownScale;

    public const float BaseEnemyMoveSpeed = 5f;
    public static float EnemyMoveSpeed => BaseEnemyMoveSpeed * MoveSpeedScale;

    // NavMesh agent + combat approach spacing (reduces stacking in chokepoints).
    // Nav bake settings (Project Settings → Navigation → Agents) must match these values.
    public const float NavAgentRadius = 0.3f;
    public static float NavBakeAgentHeight => CapsuleHeight * 2f;
    public const float ApproachSlotSpacing = 0.45f;
    public const int ApproachSlotCount = 9;
    public const float MoveOrderInterval = 0.25f;
    public const float MoveDestinationEpsilon = 0.4f;
    public const float NavAgentStoppingDistance = 0.4f;

    // Row span: (n−1)·D + (n−1)·offset. Add one diameter for full footprint edge-to-edge.
    public static float FormationUnitDiameter => CapsuleWorldRadius * 2f;
    public const float FormationOffset = 0.1f;
    public static float FormationCenterSpacing => FormationUnitDiameter + FormationOffset;

    public static float FormationWidth(int unitCount)
    {
        if (unitCount <= 1)
            return 0f;

        return (unitCount - 1) * FormationUnitDiameter + (unitCount - 1) * FormationOffset;
    }

    public static float FormationFootprintWidth(int unitCount)
    {
        if (unitCount <= 0)
            return 0f;

        return FormationWidth(unitCount) + FormationUnitDiameter;
    }

    // Bridge inner width: n·D + (n+1)·offset (end offsets + gaps between units).
    public static float FormationBridgeWidth(int unitCount)
    {
        if (unitCount <= 0)
            return 0f;

        return unitCount * FormationUnitDiameter + (unitCount + 1) * FormationOffset;
    }

    public static float LineX(int index, int count)
    {
        return LineX(index, count, SpawnSpacing);
    }

    public static float LineX(int index, int count, float spacing)
    {
        float centerOffset = (count - 1) * 0.5f;
        return (index - centerOffset) * spacing;
    }
}
