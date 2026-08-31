using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] float detectRange = 25f;
    [SerializeField] float stopDistanceFactor = 0.85f;

    UnitMovement movement;
    UnitCombat combat;
    UnitFacing facing;
    UnitTeam unitTeam;
    Health primaryTarget;
    Health currentTarget;
    Vector3 lastMoveDestination;
    float lastMoveOrderTime;

    void Awake()
    {
        movement = GetComponent<UnitMovement>();
        combat = GetComponent<UnitCombat>();
        facing = GetComponent<UnitFacing>();
        unitTeam = GetComponent<UnitTeam>();
    }

    void Update()
    {
        if (primaryTarget == null || !primaryTarget.IsAlive)
            primaryTarget = FindAttackTarget();

        if (primaryTarget == null)
        {
            currentTarget = null;
            movement?.Stop();
            return;
        }

        if (combat == null)
        {
            movement?.Stop();
            return;
        }

        currentTarget = primaryTarget;

        if (!combat.IsInRange(primaryTarget.transform))
        {
            Health blocker = UnitNavigation.FindBlockingOpponent(
                transform.position,
                primaryTarget,
                unitTeam.Team,
                gameObject);

            if (blocker != null)
                currentTarget = blocker;
        }

        Transform targetTransform = currentTarget.transform;
        combat.SetAttackTarget(currentTarget);
        facing?.FaceToward(targetTransform.position);

        if (combat.IsInRange(targetTransform))
        {
            movement?.Stop();
            combat.TryAttack(currentTarget);
            return;
        }

        float stopDistance = combat.AttackRange * stopDistanceFactor;
        Vector3 chasePosition = UnitNavigation.GetSurroundChasePosition(
            transform.position,
            targetTransform.position,
            stopDistance,
            combat.AttackRange,
            GetInstanceID());

        if (!UnitNavigation.ShouldIssueMoveOrder(chasePosition, lastMoveDestination, lastMoveOrderTime))
            return;

        lastMoveDestination = chasePosition;
        lastMoveOrderTime = Time.time;
        movement?.MoveTo(chasePosition);
    }

    static float HorizontalDistance(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        delta.y = 0f;
        return delta.magnitude;
    }

    Health FindAttackTarget()
    {
        if (unitTeam == null)
            return null;

        Health first = null;
        Health second = null;
        Health third = null;
        float firstDist = detectRange;
        float secondDist = detectRange;
        float thirdDist = detectRange;

        foreach (Health health in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (!health.IsAlive || health.gameObject == gameObject)
                continue;

            UnitTeam otherTeam = health.GetComponent<UnitTeam>();
            if (otherTeam == null || otherTeam.Team == unitTeam.Team)
                continue;

            float distance = HorizontalDistance(transform.position, health.transform.position);
            if (distance > detectRange)
                continue;

            if (distance < firstDist)
            {
                third = second;
                thirdDist = secondDist;
                second = first;
                secondDist = firstDist;
                first = health;
                firstDist = distance;
            }
            else if (distance < secondDist)
            {
                third = second;
                thirdDist = secondDist;
                second = health;
                secondDist = distance;
            }
            else if (distance < thirdDist)
            {
                third = health;
                thirdDist = distance;
            }
        }

        int candidateCount = 0;
        if (first != null)
            candidateCount++;
        if (second != null)
            candidateCount++;
        if (third != null)
            candidateCount++;

        if (candidateCount == 0)
            return null;

        int pick = UnitNavigation.Mod(GetInstanceID(), candidateCount);
        if (pick == 0)
            return first;
        if (pick == 1)
            return second;

        return third;
    }
}
