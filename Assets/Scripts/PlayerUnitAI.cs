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
    bool isShieldWall;
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
        SquadAbility ability = GetComponent<SquadAbility>();
        if (isShieldWall || (ability != null && ability.IsCharging))
            return;

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

        bool shouldAnchor = isShieldWall
            || isHolding
            || (attackTarget != null && combat != null && combat.IsInRange(attackTarget.transform))
            || (attackTarget == null && !movement.HasMoveTarget);

        movement.SetPositionAnchored(shouldAnchor);
    }

    public void HoldPosition()
    {
        isHolding = true;
        isShieldWall = false;
        ClearAttackOrder();
        movement?.Stop();
        SetDefenderBlock(true);
    }

    public void EnterShieldWall()
    {
        isHolding = true;
        isShieldWall = true;
        ClearAttackOrder();
        movement?.Stop();
    }

    public void ExitShieldWall()
    {
        isShieldWall = false;
        isHolding = false;
        SetDefenderBlock(false);
    }

    public void MoveToPosition(Vector3 position)
    {
        isHolding = false;
        isShieldWall = false;
        GetComponent<SquadAbility>()?.CancelShieldWall();
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
        isShieldWall = false;
        GetComponent<SquadAbility>()?.CancelShieldWall();
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
