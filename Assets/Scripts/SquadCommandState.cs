using System.Collections.Generic;
using UnityEngine;

public static class SquadCommandState
{
    static readonly Dictionary<UnitType, SquadCommandMode> modes = new Dictionary<UnitType, SquadCommandMode>
    {
        { UnitType.Defender, SquadCommandMode.Auto },
        { UnitType.Attacker, SquadCommandMode.Auto },
        { UnitType.Archer, SquadCommandMode.Auto }
    };

    public static void ResetAll()
    {
        modes[UnitType.Defender] = SquadCommandMode.Auto;
        modes[UnitType.Attacker] = SquadCommandMode.Auto;
        modes[UnitType.Archer] = SquadCommandMode.Auto;
        SquadRoster.Reset();
    }

    public static SquadCommandMode GetMode(UnitType type)
    {
        return modes.TryGetValue(type, out SquadCommandMode mode)
            ? mode
            : SquadCommandMode.Auto;
    }

    public static void SetMode(UnitType type, SquadCommandMode mode)
    {
        modes[type] = mode;
        ApplyModeToSquad(type, mode);
    }

    static void ApplyModeToSquad(UnitType type, SquadCommandMode mode)
    {
        foreach (UnitSelection selection in GetLivingPlayerUnitsOfType(type))
        {
            PlayerUnitAI ai = selection.GetComponent<PlayerUnitAI>();
            if (ai == null)
                continue;

            switch (mode)
            {
                case SquadCommandMode.Hold:
                    ai.HoldPosition();
                    break;
                case SquadCommandMode.Manual:
                    ai.EnterManualMode();
                    break;
                default:
                    ai.ResumeAutoBehavior();
                    break;
            }
        }
    }

    static List<UnitSelection> GetLivingPlayerUnitsOfType(UnitType type)
    {
        List<UnitSelection> units = new List<UnitSelection>();

        foreach (UnitSelection selection in Object.FindObjectsByType<UnitSelection>(FindObjectsSortMode.None))
        {
            if (selection == null)
                continue;

            UnitTeam team = selection.GetComponent<UnitTeam>();
            if (team == null || team.Team != Team.Player)
                continue;

            Health health = selection.GetComponent<Health>();
            if (health != null && !health.IsAlive)
                continue;

            Unit unit = selection.GetComponent<Unit>();
            if (unit == null || unit.Type != type)
                continue;

            units.Add(selection);
        }

        return units;
    }
}
