using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    readonly List<UnitSelection> selectedUnits = new List<UnitSelection>();

    void Update()
    {
        PruneDestroyedUnits();

        if (Mouse.current == null || Camera.main == null)
            return;

        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            HoldSelectedUnits();
            return;
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

    void HoldSelectedUnits()
    {
        PruneDestroyedUnits();

        foreach (UnitSelection unit in selectedUnits)
        {
            PlayerUnitAI ai = unit.GetComponent<PlayerUnitAI>();
            if (ai != null)
                ai.HoldPosition();
        }
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
        bool shiftHeld = Keyboard.current != null &&
            (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        if (shiftHeld)
        {
            ToggleUnitInSelection(unit);
            return;
        }

        if (selectedUnits.Count == 1 && selectedUnits[0] == unit)
        {
            DeselectAll();
            return;
        }

        SelectOnly(unit);
    }

    void ToggleUnitInSelection(UnitSelection unit)
    {
        if (unit.IsSelected)
        {
            unit.Deselect();
            selectedUnits.Remove(unit);
            return;
        }

        unit.Select();
        if (!selectedUnits.Contains(unit))
            selectedUnits.Add(unit);
    }

    void SelectOnly(UnitSelection unit)
    {
        DeselectAll();
        unit.Select();
        selectedUnits.Add(unit);
    }

    void DeselectAll()
    {
        PruneDestroyedUnits();

        foreach (UnitSelection unit in selectedUnits)
            unit.Deselect();

        selectedUnits.Clear();
    }
}
