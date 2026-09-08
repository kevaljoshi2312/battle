using UnityEngine;

public class PlayerUnitAI : MonoBehaviour
{
    [SerializeField] float defenderBlockRadius = 1.1f;
    [SerializeField] float detectRange = 25f;

    UnitMovement movement;
    UnitCombat combat;
    Unit unit;
    UnitTeam unitTeam;
    UnitFacing facing;
    CapsuleCollider capsuleCollider;
    Health attackTarget;
    Health autoTarget;
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
        unitTeam = GetComponent<UnitTeam>();
        facing = GetComponent<UnitFacing>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        if (capsuleCollider != null)
            defaultColliderRadius = capsuleCollider.radius;
    }

    void Update()
    {
        SquadAbility ability = GetComponent<SquadAbility>();
        if (isShieldWall || (ability != null && ability.IsCharging))
        {
            autoTarget = null;
            return;
        }

        if (unit != null &&
            SquadCommandState.GetMode(unit.Type) == SquadCommandMode.Hold &&
            !isHolding &&
            attackTarget == null &&
            (movement == null || !movement.IsFollowingPlayerMoveOrder))
        {
            HoldPosition();
            return;
        }

        if (isHolding)
        {
            autoTarget = null;
            return;
        }

        if (movement != null && movement.IsFollowingPlayerMoveOrder)
        {
            autoTarget = null;
            return;
        }

        if (attackTarget != null)
        {
            if (!attackTarget.IsAlive)
            {
                ClearAttackOrder();
            }
            else
            {
                autoTarget = null;
                RunCombatTick(attackTarget, useSurroundSlots: true);
                return;
            }
        }

        if (unit != null && SquadCommandState.GetMode(unit.Type) == SquadCommandMode.Manual)
        {
            autoTarget = null;
            return;
        }

        RunIdleAutoCombat();
    }

    void LateUpdate()
    {
        if (movement == null)
            return;

        movement.SetPositionAnchored(isShieldWall || isHolding);
    }

    public void HoldPosition()
    {
        isHolding = true;
        isShieldWall = false;
        autoTarget = null;
        ClearAttackOrder();
        movement?.Stop();
        SetDefenderBlock(true);
    }

    public void EnterShieldWall()
    {
        isHolding = true;
        isShieldWall = true;
        autoTarget = null;
        ClearAttackOrder();
        movement?.Stop();
    }

    public void ExitShieldWall()
    {
        isShieldWall = false;
        isHolding = false;
        SetDefenderBlock(false);
    }

    public void ResumeAutoBehavior()
    {
        isHolding = false;
        isShieldWall = false;
        autoTarget = null;
        GetComponent<SquadAbility>()?.CancelShieldWall();
        SetDefenderBlock(false);
    }

    public void EnterManualMode()
    {
        ResumeAutoBehavior();
    }

    public void MoveToPosition(Vector3 position)
    {
        isHolding = false;
        isShieldWall = false;
        autoTarget = null;
        GetComponent<SquadAbility>()?.CancelShieldWall();
        ClearAttackOrder();
        SetDefenderBlock(false);
        lastMoveDestination = position;
        lastMoveOrderTime = Time.time;
        movement?.MoveToCommand(position);
    }

    public void AttackTarget(Health target)
    {
        if (target == null || !target.IsAlive)
            return;

        isHolding = false;
        isShieldWall = false;
        autoTarget = null;
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

    void RunIdleAutoCombat()
    {
        Health target = UnitAutoCombat.ResolveTarget(
            transform.position,
            gameObject,
            unitTeam,
            detectRange,
            preferredTarget: null);

        autoTarget = RunCombatTick(target, useSurroundSlots: false);
    }

    Health RunCombatTick(Health preferredTarget, bool useSurroundSlots)
    {
        if (unitTeam == null || movement == null || combat == null)
            return null;

        Health target = UnitAutoCombat.ResolveTarget(
            transform.position,
            gameObject,
            unitTeam,
            detectRange,
            preferredTarget);

        return UnitAutoCombat.Tick(
            gameObject,
            movement,
            combat,
            facing,
            target,
            useSurroundSlots,
            GetInstanceID(),
            ref lastMoveDestination,
            ref lastMoveOrderTime);
    }

    void SetDefenderBlock(bool enabled)
    {
        if (unit == null || unit.Type != UnitType.Defender || capsuleCollider == null)
            return;

        capsuleCollider.radius = enabled ? defenderBlockRadius : defaultColliderRadius;
    }
}
