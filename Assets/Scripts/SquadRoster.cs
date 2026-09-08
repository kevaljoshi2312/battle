using System.Collections.Generic;
using UnityEngine;

public static class SquadRoster
{
    static readonly Dictionary<UnitType, int> initialMaxCounts = new Dictionary<UnitType, int>();
    static bool initialized;

    public static void Reset()
    {
        initialized = false;
        initialMaxCounts.Clear();
    }

    public static void GetCounts(UnitType type, out int alive, out int max)
    {
        EnsureInitialized();

        alive = 0;
        int present = 0;

        foreach (UnitSelection selection in Object.FindObjectsByType<UnitSelection>(FindObjectsSortMode.None))
        {
            if (selection == null)
                continue;

            UnitTeam team = selection.GetComponent<UnitTeam>();
            if (team == null || team.Team != Team.Player)
                continue;

            Unit unit = selection.GetComponent<Unit>();
            if (unit == null || unit.Type != type)
                continue;

            present++;

            Health health = selection.GetComponent<Health>();
            if (health != null && health.IsAlive)
                alive++;
        }

        if (!initialMaxCounts.TryGetValue(type, out int storedMax))
            storedMax = 0;

        max = Mathf.Max(storedMax, present, alive);
        initialMaxCounts[type] = max;
    }

    static void EnsureInitialized()
    {
        if (initialized)
            return;

        initialized = true;

        foreach (UnitType squadType in new[] { UnitType.Defender, UnitType.Attacker, UnitType.Archer })
            initialMaxCounts[squadType] = 0;

        foreach (UnitSelection selection in Object.FindObjectsByType<UnitSelection>(FindObjectsSortMode.None))
        {
            if (selection == null)
                continue;

            UnitTeam team = selection.GetComponent<UnitTeam>();
            if (team == null || team.Team != Team.Player)
                continue;

            Unit unit = selection.GetComponent<Unit>();
            if (unit == null)
                continue;

            initialMaxCounts.TryGetValue(unit.Type, out int count);
            initialMaxCounts[unit.Type] = count + 1;
        }
    }
}
