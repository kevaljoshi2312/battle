using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] float detectRange = 25f;
    [SerializeField] float stopDistanceFactor = 0.85f;

    UnitMovement movement;
    UnitCombat combat;
    UnitTeam unitTeam;
    Health currentTarget;

    void Awake()
    {
        movement = GetComponent<UnitMovement>();
        combat = GetComponent<UnitCombat>();
        unitTeam = GetComponent<UnitTeam>();
    }

    void Update()
    {
        if (currentTarget == null || !currentTarget.IsAlive)
            currentTarget = FindNearestEnemy();

        if (currentTarget == null)
        {
            movement?.Stop();
            return;
        }

        Transform targetTransform = currentTarget.transform;
        float stopDistance = combat.AttackRange * stopDistanceFactor;
        float distanceToTarget = HorizontalDistance(transform.position, targetTransform.position);

        if (distanceToTarget <= stopDistance)
        {
            movement?.Stop();
            combat.TryAttack(currentTarget);
            return;
        }

        if (movement != null)
            movement.MoveTo(GetChasePosition(targetTransform.position, stopDistance));
    }

    Vector3 GetChasePosition(Vector3 targetPosition, float stopDistance)
    {
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance <= stopDistance)
            return transform.position;

        Vector3 chasePosition = transform.position + toTarget.normalized * (distance - stopDistance);
        chasePosition.y = transform.position.y;
        return chasePosition;
    }

    static float HorizontalDistance(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        delta.y = 0f;
        return delta.magnitude;
    }

    Health FindNearestEnemy()
    {
        if (unitTeam == null)
            return null;

        Health nearest = null;
        float nearestDistance = detectRange;

        foreach (Health health in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (!health.IsAlive || health.gameObject == gameObject)
                continue;

            UnitTeam otherTeam = health.GetComponent<UnitTeam>();
            if (otherTeam == null || otherTeam.Team == unitTeam.Team)
                continue;

            float distance = HorizontalDistance(transform.position, health.transform.position);
            if (distance > nearestDistance)
                continue;

            nearest = health;
            nearestDistance = distance;
        }

        return nearest;
    }
}
