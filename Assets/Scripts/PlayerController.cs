using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    readonly List<UnitSelection> selectedUnits = new List<UnitSelection>();

    public IReadOnlyList<UnitSelection> SelectedUnits => selectedUnits;

    public IReadOnlyList<UnitType> SelectedSquads => GetSelectedSquadTypes();

    void Update()
    {
        PruneDestroyedUnits();

        if (Mouse.current == null || Camera.main == null)
            return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                ActivateShieldWall();
                return;
            }

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                ActivateCharge();
                return;
            }

            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                ActivateVolley();
                return;
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            DeselectAll();
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length == 0)
            return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        UnitSelection clickedUnit = FindClosestPlayerUnit(hits);
        if (clickedUnit != null)
        {
            HandleUnitClick(clickedUnit);
            return;
        }

        if (selectedUnits.Count == 0)
            return;

        Health clickedEnemy = FindClosestEnemy(hits);
        if (clickedEnemy != null)
        {
            AttackWithSelectedUnits(clickedEnemy);
            return;
        }

        if (!TryFindGroundPoint(hits, out Vector3 groundPoint))
            return;

        MoveSelectedUnitsTo(groundPoint);
    }

    void ActivateShieldWall()
    {
        PruneDestroyedUnits();
        if (selectedUnits.Count == 0)
            return;

        foreach (UnitSelection unit in selectedUnits)
        {
            SquadAbility ability = unit.GetComponent<SquadAbility>();
            if (ability != null && ability.TryActivateShieldWall())
                continue;

            PlayerUnitAI ai = unit.GetComponent<PlayerUnitAI>();
            if (ai != null)
                ai.HoldPosition();
        }
    }

    void ActivateCharge()
    {
        PruneDestroyedUnits();
        if (selectedUnits.Count == 0 || !TryGetWorldTargetFromMouse(out Vector3 destination, out Health enemyTarget))
            return;

        foreach (UnitSelection unit in selectedUnits)
        {
            SquadAbility ability = unit.GetComponent<SquadAbility>();
            ability?.TryActivateCharge(destination, enemyTarget);
        }
    }

    void ActivateVolley()
    {
        PruneDestroyedUnits();
        if (selectedUnits.Count == 0 || !TryGetGroundPointFromMouse(out Vector3 volleyCenter))
            return;

        foreach (UnitSelection unit in selectedUnits)
        {
            SquadAbility ability = unit.GetComponent<SquadAbility>();
            ability?.TryActivateVolley(volleyCenter);
        }
    }

    bool TryGetWorldTargetFromMouse(out Vector3 destination, out Health enemyTarget)
    {
        destination = default;
        enemyTarget = null;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length == 0)
            return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        enemyTarget = FindClosestEnemy(hits);

        if (enemyTarget != null)
        {
            destination = enemyTarget.transform.position;
            return true;
        }

        return TryFindGroundPoint(hits, out destination);
    }

    bool TryGetGroundPointFromMouse(out Vector3 point)
    {
        point = default;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length == 0)
            return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        return TryFindGroundPoint(hits, out point);
    }

    void AttackWithSelectedUnits(Health target)
    {
        PruneDestroyedUnits();

        foreach (UnitSelection unit in selectedUnits)
        {
            PlayerUnitAI ai = unit.GetComponent<PlayerUnitAI>();
            if (ai != null)
                ai.AttackTarget(target);
        }
    }

    void MoveSelectedUnitsTo(Vector3 destination)
    {
        PruneDestroyedUnits();
        if (selectedUnits.Count == 0)
            return;

        if (selectedUnits.Count == 1)
        {
            IssueMoveOrder(selectedUnits[0], destination);
            return;
        }

        Vector3 groupCenter = Vector3.zero;
        foreach (UnitSelection unit in selectedUnits)
            groupCenter += unit.transform.position;
        groupCenter /= selectedUnits.Count;

        List<UnitSelection> orderedUnits = new List<UnitSelection>(selectedUnits);
        orderedUnits.Sort((a, b) =>
        {
            float angleA = Mathf.Atan2(
                a.transform.position.z - groupCenter.z,
                a.transform.position.x - groupCenter.x);
            float angleB = Mathf.Atan2(
                b.transform.position.z - groupCenter.z,
                b.transform.position.x - groupCenter.x);
            return angleA.CompareTo(angleB);
        });

        const float unitSpacing = UnitVisuals.GroupMoveSpacing;
        int count = orderedUnits.Count;
        float radius = unitSpacing / (2f * Mathf.Sin(Mathf.PI / count));

        for (int i = 0; i < count; i++)
        {
            UnitSelection unit = orderedUnits[i];
            float angle = (2f * Mathf.PI * i) / count;
            Vector3 target = destination + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            target.y = unit.transform.position.y;
            IssueMoveOrder(unit, target);
        }
    }

    static void IssueMoveOrder(UnitSelection unit, Vector3 destination)
    {
        PlayerUnitAI ai = unit.GetComponent<PlayerUnitAI>();
        if (ai != null)
        {
            ai.MoveToPosition(destination);
            return;
        }

        UnitMovement movement = unit.GetComponent<UnitMovement>();
        if (movement != null)
            movement.MoveTo(destination);
    }

    static UnitSelection FindClosestPlayerUnit(RaycastHit[] hits)
    {
        UnitSelection closest = null;
        float closestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            UnitSelection unit = hit.collider.GetComponentInParent<UnitSelection>();
            if (unit == null || hit.distance >= closestDistance)
                continue;

            UnitTeam team = unit.GetComponent<UnitTeam>();
            if (team != null && team.Team != Team.Player)
                continue;

            closest = unit;
            closestDistance = hit.distance;
        }

        return closest;
    }

    static Health FindClosestEnemy(RaycastHit[] hits)
    {
        Health closest = null;
        float closestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            Health health = hit.collider.GetComponentInParent<Health>();
            if (health == null || !health.IsAlive || hit.distance >= closestDistance)
                continue;

            UnitTeam team = health.GetComponent<UnitTeam>();
            if (team == null || team.Team != Team.Enemy)
                continue;

            closest = health;
            closestDistance = hit.distance;
        }

        return closest;
    }

    static bool TryFindGroundPoint(RaycastHit[] hits, out Vector3 point)
    {
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.GetComponentInParent<Health>() != null)
                continue;

            point = hit.point;
            return true;
        }

        point = default;
        return false;
    }

    void PruneDestroyedUnits()
    {
        selectedUnits.RemoveAll(unit => unit == null);
    }

    void HandleUnitClick(UnitSelection unit)
    {
        Unit unitProfile = unit.GetComponent<Unit>();
        UnitType squadType = unitProfile != null
            ? unitProfile.Type
            : UnitType.Defender;

        bool shiftHeld = Keyboard.current != null &&
            (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        if (shiftHeld)
        {
            ToggleSquadInSelection(squadType);
            return;
        }

        if (IsOnlySquadSelected(squadType))
        {
            DeselectAll();
            return;
        }

        SelectSquadOnly(squadType);
    }

    void SelectSquadOnly(UnitType type)
    {
        DeselectAll();

        foreach (UnitSelection unit in GetLivingPlayerUnitsOfType(type))
        {
            unit.Select();
            selectedUnits.Add(unit);
        }
    }

    void ToggleSquadInSelection(UnitType type)
    {
        List<UnitSelection> squad = GetLivingPlayerUnitsOfType(type);
        if (squad.Count == 0)
            return;

        bool allSelected = true;
        foreach (UnitSelection unit in squad)
        {
            if (!unit.IsSelected)
            {
                allSelected = false;
                break;
            }
        }

        if (allSelected)
        {
            foreach (UnitSelection unit in squad)
            {
                unit.Deselect();
                selectedUnits.Remove(unit);
            }

            return;
        }

        foreach (UnitSelection unit in squad)
        {
            if (unit.IsSelected)
                continue;

            unit.Select();
            if (!selectedUnits.Contains(unit))
                selectedUnits.Add(unit);
        }
    }

    bool IsOnlySquadSelected(UnitType type)
    {
        List<UnitSelection> squad = GetLivingPlayerUnitsOfType(type);
        if (squad.Count == 0 || selectedUnits.Count != squad.Count)
            return false;

        foreach (UnitSelection unit in squad)
        {
            if (!unit.IsSelected)
                return false;
        }

        return true;
    }

    List<UnitType> GetSelectedSquadTypes()
    {
        List<UnitType> squadTypes = new List<UnitType>();
        PruneDestroyedUnits();

        foreach (UnitSelection unit in selectedUnits)
        {
            Unit unitProfile = unit.GetComponent<Unit>();
            if (unitProfile == null)
                continue;

            if (!squadTypes.Contains(unitProfile.Type))
                squadTypes.Add(unitProfile.Type);
        }

        return squadTypes;
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

    void DeselectAll()
    {
        PruneDestroyedUnits();

        foreach (UnitSelection unit in selectedUnits)
            unit.Deselect();

        selectedUnits.Clear();
    }
}
