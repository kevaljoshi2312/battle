using UnityEngine;

public class PlayerUnitAI : MonoBehaviour
{
    [SerializeField] float defenderBlockRadius = 1.1f;

    UnitMovement movement;
    UnitCombat combat;
    Unit unit;
    UnitFacing facing;
    CapsuleCollider capsuleCollider;
    Health attackTarget;
    bool isHolding;
    float defaultColliderRadius;
    Vector3 lastMoveDestination;
    float lastMoveOrderTime;

    public bool IsHolding => isHolding;

    void Awake()
    {
        movement = GetComponent<UnitMovement>();
        combat = GetComponent<UnitCombat>();
        unit = GetComponent<Unit>();
        facing = GetComponent<UnitFacing>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        if (capsuleCollider != null)
            defaultColliderRadius = capsuleCollider.radius;
    }

    void Update()
    {
        if (attackTarget == null)
            return;

        if (!attackTarget.IsAlive)
        {
            ClearAttackOrder();
            return;
        }

        ChaseAndAttack(attackTarget);
    }

    void LateUpdate()
    {
        if (movement == null)
            return;

        bool shouldAnchor = isHolding
            || (attackTarget != null && combat != null && combat.IsInRange(attackTarget.transform))
            || (attackTarget == null && !movement.HasMoveTarget);

        movement.SetPositionAnchored(shouldAnchor);
    }

    public void HoldPosition()
    {
        isHolding = true;
        ClearAttackOrder();
        movement?.Stop();
        SetDefenderBlock(true);
    }

    public void MoveToPosition(Vector3 position)
    {
        isHolding = false;
        ClearAttackOrder();
        SetDefenderBlock(false);
        lastMoveDestination = position;
        lastMoveOrderTime = Time.time;
        movement?.MoveTo(position);
    }

    public void AttackTarget(Health target)
    {
        if (target == null || !target.IsAlive)
            return;

        isHolding = false;
        attackTarget = target;
        combat?.SetAttackTarget(target);
        SetDefenderBlock(false);
    }

    void ClearAttackOrder()
    {
        attackTarget = null;
        combat?.ClearAttackTarget();
    }

    void ChaseAndAttack(Health target)
    {
        if (combat == null)
            return;

        Transform targetTransform = target.transform;
        facing?.FaceToward(targetTransform.position);

        float stopDistance = GetStopDistance();

        if (combat.IsInRange(targetTransform))
        {
            movement?.Stop();
            combat.TryAttack(target);
            return;
        }

        Vector3 chasePosition = GetChasePosition(targetTransform.position, stopDistance);
        if (!UnitNavigation.ShouldIssueMoveOrder(chasePosition, lastMoveDestination, lastMoveOrderTime))
            return;

        lastMoveDestination = chasePosition;
        lastMoveOrderTime = Time.time;
        movement?.MoveTo(chasePosition);
    }

    float GetStopDistance()
    {
        if (combat == null)
            return 0f;

        return UnitVisuals.GetChaseStopDistance(combat.AttackRange);
    }

    Vector3 GetChasePosition(Vector3 targetPosition, float stopDistance)
    {
        return UnitNavigation.GetSurroundChasePosition(
            transform.position,
            targetPosition,
            stopDistance,
            combat.AttackRange,
            GetInstanceID());
    }

    void SetDefenderBlock(bool enabled)
    {
        if (unit == null || unit.Type != UnitType.Defender || capsuleCollider == null)
            return;

        capsuleCollider.radius = enabled ? defenderBlockRadius : defaultColliderRadius;
    }

    static float HorizontalDistance(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        delta.y = 0f;
        return delta.magnitude;
    }
}
