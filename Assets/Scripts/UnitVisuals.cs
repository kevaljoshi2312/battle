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
    public const float ArrowSpeed = 14f;
    public const float ArrowHitDistance = 0.35f;
    public const float ArrowSpawnHeight = 0.45f;
    public const float ArrowTargetHeight = 0.45f;
    public static readonly Color ArrowColor = new Color(0.85f, 0.65f, 0.2f);
    public static readonly Vector3 ArrowScale = new Vector3(0.04f, 0.04f, 0.35f);

    public static bool IsRangedAttack(float attackRange) => attackRange > MeleeAttackRangeMax;

    public const float MoveSpeedScale = 0.5f;
    public const float BaseTurnSpeed = 720f;
    public static float TurnSpeed => BaseTurnSpeed * MoveSpeedScale;

    public const float BaseAttackCooldown = 1f;
    public const float AttackCooldownScale = 2f;
    public static float AttackCooldown => BaseAttackCooldown * AttackCooldownScale;

    public const float BaseEnemyMoveSpeed = 5f;
    public static float EnemyMoveSpeed => BaseEnemyMoveSpeed * MoveSpeedScale;

    // NavMesh agent + combat approach spacing (reduces stacking in chokepoints).
    public static float NavAgentRadius => CapsuleWorldRadius;
    public const float ApproachSlotSpacing = 0.45f;
    public const int ApproachSlotCount = 9;
    public const float MoveOrderInterval = 0.25f;
    public const float MoveDestinationEpsilon = 0.4f;
    public const float NavAgentStoppingDistance = 0.4f;

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
