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
    EnemyAI enemyAI;
    PlayerUnitAI playerUnitAI;

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
        enemyAI = GetComponent<EnemyAI>();
        playerUnitAI = GetComponent<PlayerUnitAI>();
    }

    void Update()
    {
        // EnemyAI / PlayerUnitAI own targeting and attacks.
        if (enemyAI != null || playerUnitAI != null)
            return;

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
            DealMeleeDamage(target);

        nextAttackTime = Time.time + attackCooldown;
    }

    void DealMeleeDamage(Health target)
    {
        int totalDamage = damage;
        DamageModifier modifier = GetComponent<DamageModifier>();
        if (modifier != null)
        {
            totalDamage = Mathf.RoundToInt(damage * modifier.OutgoingDamageMultiplier);
            if (modifier.BonusDamageOnNextHit > 0)
            {
                totalDamage += modifier.BonusDamageOnNextHit;
                modifier.ConsumeBonusDamageOnHit();
            }
        }

        int baseDamage = totalDamage;
        totalDamage = FlankingCombat.ApplyFlankingDamage(
            totalDamage,
            transform.position,
            target,
            out FlankType flankType);

        if (unitTeam != null && unitTeam.Team == Team.Player)
            ShowFlankFeedback(flankType);

        target.TakeDamage(totalDamage);
    }

    void ShowFlankFeedback(FlankType flankType)
    {
        string label = FlankingCombat.GetFlankLabel(flankType);
        if (!string.IsNullOrEmpty(label))
            AbilityFeedback.Show(label, 1.2f);
    }

    void FireProjectile(Health target)
    {
        Vector3 spawnPosition = transform.position + Vector3.up * UnitVisuals.ArrowSpawnHeight;
        GameObject arrowObject = new GameObject("Arrow");
        Arrow arrow = arrowObject.AddComponent<Arrow>();
        Team team = unitTeam != null ? unitTeam.Team : Team.Player;
        arrow.Launch(
            spawnPosition,
            target,
            damage,
            UnitVisuals.ArrowSpeed,
            team,
            showFlankFeedback: team == Team.Player);
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
