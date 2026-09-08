using UnityEngine;

public readonly struct SquadAbilitySnapshot
{
    public bool HasLivingUnits { get; }
    public bool IsActive { get; }
    public float ActiveRemaining { get; }
    public float CooldownRemaining { get; }

    public SquadAbilitySnapshot(
        bool hasLivingUnits,
        bool isActive,
        float activeRemaining,
        float cooldownRemaining)
    {
        HasLivingUnits = hasLivingUnits;
        IsActive = isActive;
        ActiveRemaining = activeRemaining;
        CooldownRemaining = cooldownRemaining;
    }

    public bool IsReady => HasLivingUnits && !IsActive && CooldownRemaining <= 0f;
}

public static class SquadAbilityState
{
    public static SquadAbilitySnapshot GetForSquad(UnitType squadType)
    {
        bool hasLivingUnits = false;
        bool isActive = false;
        float activeRemaining = 0f;
        float cooldownRemaining = 0f;

        foreach (SquadAbility ability in Object.FindObjectsByType<SquadAbility>(FindObjectsSortMode.None))
        {
            if (ability == null || !ability.IsPlayerUnitAlive())
                continue;

            Unit unit = ability.GetComponent<Unit>();
            if (unit == null || unit.Type != squadType)
                continue;

            hasLivingUnits = true;
            SquadAbilitySnapshot unitSnapshot = ability.GetSnapshot();

            if (unitSnapshot.IsActive)
            {
                isActive = true;
                activeRemaining = Mathf.Max(activeRemaining, unitSnapshot.ActiveRemaining);
            }

            cooldownRemaining = Mathf.Max(cooldownRemaining, unitSnapshot.CooldownRemaining);
        }

        return new SquadAbilitySnapshot(
            hasLivingUnits,
            isActive,
            activeRemaining,
            cooldownRemaining);
    }
}
