using UnityEngine;

public class SquadAbility : MonoBehaviour
{
    const float ShieldWallDuration = 8f;
    const float ShieldWallCooldown = 15f;
    const float ShieldWallDamageMultiplier = 0.35f;
    const float ShieldWallBlockRadius = 1.35f;

    const float ChargeDuration = 2.5f;
    const float ChargeCooldown = 12f;
    const float ChargeSpeedMultiplier = 2.2f;
    const int ChargeBonusDamage = 14;

    const float VolleyCooldown = 18f;
    const float VolleyRadius = 2.5f;
    const int VolleyDamagePerArcher = 5;

    public static float ShieldWallDurationSeconds => ShieldWallDuration;
    public static float ShieldWallCooldownSeconds => ShieldWallCooldown;
    public static float ChargeDurationSeconds => ChargeDuration;
    public static float ChargeCooldownSeconds => ChargeCooldown;
    public static float VolleyCooldownSeconds => VolleyCooldown;

    Unit unit;
    Health health;
    PlayerUnitAI unitAI;
    UnitCombat combat;
    UnitMovement movement;
    DamageModifier damageModifier;
    CapsuleCollider capsuleCollider;
    float defaultColliderRadius;
    float defaultMoveSpeed;

    float shieldWallEndTime;
    float shieldWallCooldownEnd;
    float chargeEndTime;
    float chargeCooldownEnd;
    float volleyCooldownEnd;
    bool isCharging;
    Health chargeTarget;
    Vector3 chargeDestination;

    public bool IsShieldWallActive => Time.time < shieldWallEndTime;
    public bool IsCharging => isCharging;

    public bool IsPlayerUnitAlive()
    {
        if (health == null)
            return false;

        UnitTeam team = GetComponent<UnitTeam>();
        return health.IsAlive && team != null && team.Team == Team.Player;
    }

    public SquadAbilitySnapshot GetSnapshot()
    {
        if (unit == null || health == null || !health.IsAlive)
            return default;

        float activeRemaining = 0f;
        bool isActive = false;

        if (IsShieldWallActive)
        {
            isActive = true;
            activeRemaining = Mathf.Max(activeRemaining, shieldWallEndTime - Time.time);
        }

        if (IsCharging)
        {
            isActive = true;
            activeRemaining = Mathf.Max(activeRemaining, chargeEndTime - Time.time);
        }

        float cooldownRemaining = GetCooldownRemaining();

        return new SquadAbilitySnapshot(
            hasLivingUnits: true,
            isActive: isActive,
            activeRemaining: activeRemaining,
            cooldownRemaining: cooldownRemaining);
    }

    float GetCooldownRemaining()
    {
        if (unit == null)
            return 0f;

        return unit.Type switch
        {
            UnitType.Defender => Mathf.Max(0f, shieldWallCooldownEnd - Time.time),
            UnitType.Attacker => Mathf.Max(0f, chargeCooldownEnd - Time.time),
            UnitType.Archer => Mathf.Max(0f, volleyCooldownEnd - Time.time),
            _ => 0f
        };
    }

    public float GetCooldownTotal()
    {
        if (unit == null)
            return 1f;

        return unit.Type switch
        {
            UnitType.Defender => ShieldWallCooldown,
            UnitType.Attacker => ChargeCooldown,
            UnitType.Archer => VolleyCooldown,
            _ => 1f
        };
    }

    void Awake()
    {
        unit = GetComponent<Unit>();
        health = GetComponent<Health>();
        unitAI = GetComponent<PlayerUnitAI>();
        combat = GetComponent<UnitCombat>();
        movement = GetComponent<UnitMovement>();
        damageModifier = GetComponent<DamageModifier>();
        if (damageModifier == null)
            damageModifier = gameObject.AddComponent<DamageModifier>();

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
            defaultColliderRadius = capsule.radius;

        if (movement != null)
            defaultMoveSpeed = movement.MoveSpeed;
    }

    void Update()
    {
        if (shieldWallEndTime > 0f && Time.time >= shieldWallEndTime)
            CancelShieldWall();

        if (!isCharging)
            return;

        if (Time.time >= chargeEndTime)
        {
            EndCharge(false);
            return;
        }

        if (chargeTarget != null && chargeTarget.IsAlive && combat != null && combat.IsInRange(chargeTarget.transform))
        {
            combat.TryAttack(chargeTarget);
            EndCharge(true);
        }
    }

    public bool TryActivateShieldWall()
    {
        if (unit == null || unit.Type != UnitType.Defender || health == null || !health.IsAlive)
            return false;

        if (Time.time < shieldWallCooldownEnd)
            return false;

        shieldWallEndTime = Time.time + ShieldWallDuration;
        shieldWallCooldownEnd = Time.time + ShieldWallCooldown;
        damageModifier.IncomingDamageMultiplier = ShieldWallDamageMultiplier;
        ExpandBlockCollider(true);
        unitAI?.EnterShieldWall();
        return true;
    }

    public bool TryActivateCharge(Vector3 destination, Health priorityTarget)
    {
        if (unit == null || unit.Type != UnitType.Attacker || health == null || !health.IsAlive)
            return false;

        if (isCharging || Time.time < chargeCooldownEnd)
            return false;

        isCharging = true;
        chargeTarget = priorityTarget;
        chargeDestination = destination;
        chargeEndTime = Time.time + ChargeDuration;
        chargeCooldownEnd = Time.time + ChargeCooldown;
        damageModifier.BonusDamageOnNextHit = ChargeBonusDamage;

        if (movement != null)
        {
            movement.Configure(defaultMoveSpeed * ChargeSpeedMultiplier);
            movement.MoveTo(destination);
        }

        if (priorityTarget != null)
            unitAI?.AttackTarget(priorityTarget);

        return true;
    }

    public bool TryActivateVolley(Vector3 center)
    {
        if (unit == null || unit.Type != UnitType.Archer || health == null || !health.IsAlive)
            return false;

        if (Time.time < volleyCooldownEnd)
            return false;

        volleyCooldownEnd = Time.time + VolleyCooldown;
        ApplyVolleyDamage(center);
        SpawnVolleyArrows(center);
        return true;
    }

    public void CancelShieldWall()
    {
        if (shieldWallEndTime <= 0f && damageModifier.IncomingDamageMultiplier >= 1f)
            return;

        shieldWallEndTime = 0f;
        damageModifier.IncomingDamageMultiplier = 1f;
        ExpandBlockCollider(false);
        unitAI?.ExitShieldWall();
    }

    void EndCharge(bool landedHit)
    {
        isCharging = false;
        chargeTarget = null;

        if (movement != null)
            movement.Configure(defaultMoveSpeed);

        if (!landedHit)
            damageModifier.ConsumeBonusDamageOnHit();
    }

    void ApplyVolleyDamage(Vector3 center)
    {
        foreach (Health target in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (!target.IsAlive)
                continue;

            UnitTeam team = target.GetComponent<UnitTeam>();
            if (team == null || team.Team != Team.Enemy)
                continue;

            if (HorizontalDistance(target.transform.position, center) > VolleyRadius)
                continue;

            target.TakeDamage(VolleyDamagePerArcher);
        }
    }

    void SpawnVolleyArrows(Vector3 center)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 offset = Random.insideUnitCircle * VolleyRadius * 0.8f;
            Vector3 impactPoint = center + new Vector3(offset.x, 0f, offset.y);

            Vector3 launchPosition = transform.position + Vector3.up * UnitVisuals.ArrowSpawnHeight;
            GameObject arrowObject = new GameObject("VolleyArrow");
            arrowObject.transform.position = launchPosition;
            VolleyArrow volleyArrow = arrowObject.AddComponent<VolleyArrow>();
            volleyArrow.Launch(launchPosition, impactPoint, UnitVisuals.ArrowSpeed);
        }
    }

    void ExpandBlockCollider(bool enabled)
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null)
            return;

        capsule.radius = enabled ? ShieldWallBlockRadius : defaultColliderRadius;
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        Vector3 delta = a - b;
        delta.y = 0f;
        return delta.magnitude;
    }
}
