using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [SerializeField] int damage = 10;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] bool autoAttackWhenInRange;

    float nextAttackTime;
    UnitTeam unitTeam;
    UnitFacing facing;
    Health focusTarget;

    public float AttackRange => attackRange;

    public void Configure(int damageAmount, float range, bool autoAttack = true)
    {
        damage = damageAmount;
        attackRange = range;
        autoAttackWhenInRange = autoAttack;
    }

    public void SetAttackTarget(Health target)
    {
        focusTarget = target;
    }

    public void ClearAttackTarget()
    {
        focusTarget = null;
    }

    void Awake()
    {
        attackCooldown = UnitVisuals.AttackCooldown;
        unitTeam = GetComponent<UnitTeam>();
        facing = GetComponent<UnitFacing>();
    }

    void Update()
    {
        Health target = GetAttackTarget();
        if (target == null)
            return;

        facing?.FaceToward(target.transform.position);
        TryAttack(target);
    }

    Health GetAttackTarget()
    {
        if (focusTarget != null)
        {
            if (!focusTarget.IsAlive)
                focusTarget = null;
            else
                return focusTarget;
        }

        if (!autoAttackWhenInRange)
            return null;

        return FindNearestEnemyInRange();
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

        if (UnitVisuals.IsRangedAttack(attackRange))
            FireProjectile(target);
        else
            target.TakeDamage(damage);

        nextAttackTime = Time.time + attackCooldown;
    }

    void FireProjectile(Health target)
    {
        Vector3 spawnPosition = transform.position + Vector3.up * UnitVisuals.ArrowSpawnHeight;
        GameObject arrowObject = new GameObject("Arrow");
        Arrow arrow = arrowObject.AddComponent<Arrow>();
        arrow.Launch(spawnPosition, target, damage, UnitVisuals.ArrowSpeed);
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
