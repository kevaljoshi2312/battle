using UnityEngine;

[DefaultExecutionOrder(-50)]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] float detectRange = 25f;

    UnitMovement movement;
    UnitCombat combat;
    UnitFacing facing;
    UnitTeam unitTeam;
    Health primaryTarget;
    Health currentTarget;
    Vector3 lastMoveDestination;
    float lastMoveOrderTime;

    public Health PrimaryTarget => primaryTarget;
    public Health CurrentTarget => currentTarget;

    void Awake()
    {
        movement = GetComponent<UnitMovement>();
        combat = GetComponent<UnitCombat>();
        facing = GetComponent<UnitFacing>();
        unitTeam = GetComponent<UnitTeam>();
    }

    void Update()
    {
        if (unitTeam == null || movement == null || combat == null)
            return;

        primaryTarget = UnitNavigation.FindClosestOpponent(
            transform.position,
            unitTeam.Team,
            gameObject,
            detectRange);

        currentTarget = UnitAutoCombat.ResolveTarget(
            transform.position,
            gameObject,
            unitTeam,
            detectRange,
            primaryTarget);

        if (currentTarget == null)
        {
            movement.Stop();
            combat.ClearAttackTarget();
            return;
        }

        currentTarget = UnitAutoCombat.Tick(
            gameObject,
            movement,
            combat,
            facing,
            currentTarget,
            useSurroundSlots: false,
            GetInstanceID(),
            ref lastMoveDestination,
            ref lastMoveOrderTime);
    }
}
