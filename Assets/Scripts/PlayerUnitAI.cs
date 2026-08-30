using UnityEngine;

public class PlayerUnitAI : MonoBehaviour
{
    [SerializeField] float stopDistanceFactor = 0.85f;
    [SerializeField] float archerStopDistanceFactor = 0.95f;
    [SerializeField] float defenderBlockRadius = 1.1f;

    UnitMovement movement;
    UnitCombat combat;
    Unit unit;
    UnitFacing facing;
    CapsuleCollider capsuleCollider;
    Health attackTarget;
    bool isHolding;
    float defaultColliderRadius;

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
        float distanceToTarget = HorizontalDistance(transform.position, targetTransform.position);

        if (distanceToTarget <= stopDistance)
        {
            movement?.Stop();
            combat.TryAttack(target);
            return;
        }

        movement?.MoveTo(GetChasePosition(targetTransform.position, stopDistance));
    }

    float GetStopDistance()
    {
        if (combat == null)
            return 0f;

        float factor = unit != null && unit.Type == UnitType.Archer
            ? archerStopDistanceFactor
            : stopDistanceFactor;

        return combat.AttackRange * factor;
    }

    Vector3 GetChasePosition(Vector3 targetPosition, float stopDistance)
    {
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance <= stopDistance || distance <= Mathf.Epsilon)
            return transform.position;

        Vector3 chasePosition = transform.position + toTarget.normalized * (distance - stopDistance);
        chasePosition.y = transform.position.y;
        return chasePosition;
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
