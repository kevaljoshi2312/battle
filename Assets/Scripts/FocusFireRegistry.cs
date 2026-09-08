using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class FocusFireRegistry
{
    static readonly Dictionary<UnitType, Health> squadTargets = new Dictionary<UnitType, Health>();

    public static void SetFocus(UnitType squadType, Health target)
    {
        if (target == null || !target.IsAlive)
            squadTargets.Remove(squadType);
        else
            squadTargets[squadType] = target;

        RefreshHighlights();
    }

    public static void ClearAll()
    {
        squadTargets.Clear();
        RefreshHighlights();
    }

    public static void PruneDeadTargets()
    {
        List<UnitType> stale = null;

        foreach (KeyValuePair<UnitType, Health> entry in squadTargets)
        {
            if (entry.Value != null && entry.Value.IsAlive)
                continue;

            stale ??= new List<UnitType>();
            stale.Add(entry.Key);
        }

        if (stale == null)
            return;

        foreach (UnitType squadType in stale)
            squadTargets.Remove(squadType);

        RefreshHighlights();
    }

    public static Health GetFocus(UnitType squadType)
    {
        PruneDeadTargets();

        if (!squadTargets.TryGetValue(squadType, out Health target))
            return null;

        return target != null && target.IsAlive ? target : null;
    }

    public static string GetSummary()
    {
        PruneDeadTargets();
        if (squadTargets.Count == 0)
            return string.Empty;

        StringBuilder builder = new StringBuilder("FOCUS  ");
        bool first = true;

        foreach (KeyValuePair<UnitType, Health> entry in squadTargets)
        {
            if (entry.Value == null || !entry.Value.IsAlive)
                continue;

            if (!first)
                builder.Append("  |  ");

            first = false;
            builder.Append(GetSquadShortName(entry.Key));
            builder.Append(" → ");
            builder.Append(UnitIdentity.GetLabel(entry.Value));
        }

        return builder.ToString();
    }

    public static bool IsFocused(Health target)
    {
        if (target == null)
            return false;

        foreach (Health focused in squadTargets.Values)
        {
            if (focused == target)
                return true;
        }

        return false;
    }

    static void RefreshHighlights()
    {
        HashSet<Health> focusedTargets = new HashSet<Health>();
        foreach (Health target in squadTargets.Values)
        {
            if (target != null && target.IsAlive)
                focusedTargets.Add(target);
        }

        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            UnitTeam team = health.GetComponent<UnitTeam>();
            if (team == null || team.Team != Team.Enemy)
                continue;

            bool shouldShow = focusedTargets.Contains(health);
            FocusFireHighlight highlight = health.GetComponent<FocusFireHighlight>();
            if (highlight == null && shouldShow)
                highlight = health.gameObject.AddComponent<FocusFireHighlight>();

            if (highlight != null)
                highlight.SetActive(shouldShow);
        }
    }

    static string GetSquadShortName(UnitType squadType)
    {
        return squadType switch
        {
            UnitType.Defender => "DEF",
            UnitType.Attacker => "ATK",
            UnitType.Archer => "ARC",
            _ => squadType.ToString().ToUpper()
        };
    }
}
