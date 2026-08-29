using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [SerializeField] int damage = 10;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] bool autoAttackWhenInRange;

    float nextAttackTime;
    UnitTeam unitTeam;

    public float AttackRange => attackRange;

    void Awake()
    {
        unitTeam = GetComponent<UnitTeam>();
    }

    void Update()
    {
        if (!autoAttackWhenInRange)
            return;

        Health target = FindNearestEnemyInRange();
        if (target != null)
            TryAttack(target);
    }

    public bool IsInRange(Transform target)
    {
        if (target == null)
            return false;

        Vector3 delta = target.position - transform.position;
        delta.y = 0f;
        return delta.magnitude <= attackRange;
    }

    public void TryAttack(Health target)
    {
        if (target == null || !target.IsAlive)
            return;

        if (Time.time < nextAttackTime)
            return;

        if (!IsInRange(target.transform))
            return;

        target.TakeDamage(damage);
        nextAttackTime = Time.time + attackCooldown;
    }

    Health FindNearestEnemyInRange()
    {
        if (unitTeam == null)
            return null;

        Health nearest = null;
        float nearestDistance = attackRange;

        foreach (Health health in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (!health.IsAlive || health.gameObject == gameObject)
                continue;

            UnitTeam otherTeam = health.GetComponent<UnitTeam>();
            if (otherTeam == null || otherTeam.Team == unitTeam.Team)
                continue;

            Vector3 delta = health.transform.position - transform.position;
            delta.y = 0f;
            float distance = delta.magnitude;
            if (distance > nearestDistance)
                continue;

            nearest = health;
            nearestDistance = distance;
        }

        return nearest;
    }
}
