using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    enum AbilityTargetingMode
    {
        None,
        Charge,
        Volley
    }

    readonly List<UnitSelection> selectedUnits = new List<UnitSelection>();
    AbilityTargetingMode abilityTargetingMode = AbilityTargetingMode.None;

    public IReadOnlyList<UnitSelection> SelectedUnits => selectedUnits;
    public bool IsChargeTargetingPending => abilityTargetingMode == AbilityTargetingMode.Charge;
    public bool IsVolleyTargetingPending => abilityTargetingMode == AbilityTargetingMode.Volley;

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
                CancelAbilityTargeting();
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
            CancelAbilityTargeting();
            DeselectAll();
            return;
        }

        if (abilityTargetingMode != AbilityTargetingMode.None &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryExecutePendingAbilityClick();
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
            CancelAbilityTargeting();
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
        {
            AbilityFeedback.Show("Select a squad first");
            return;
        }

        int activated = 0;
        int held = 0;
        int onCooldown = 0;

        foreach (UnitSelection unit in selectedUnits)
        {
            SquadAbility ability = unit.GetComponent<SquadAbility>();
            Unit unitProfile = unit.GetComponent<Unit>();

            if (ability != null && ability.TryActivateShieldWall())
            {
                activated++;
                continue;
            }

            if (unitProfile != null && unitProfile.Type == UnitType.Defender &&
                ability != null && ability.GetSnapshot().CooldownRemaining > 0f)
            {
                onCooldown++;
                continue;
            }

            PlayerUnitAI ai = unit.GetComponent<PlayerUnitAI>();
            if (ai != null)
            {
                ai.HoldPosition();
                held++;
            }
        }

        if (activated > 0)
            AbilityFeedback.Show("Shield Wall active");
        else if (onCooldown > 0)
            AbilityFeedback.Show("Shield Wall on cooldown");
        else if (held > 0)
            AbilityFeedback.Show("Archers holding position");
    }

    void ActivateCharge()
    {
        PruneDestroyedUnits();
        if (selectedUnits.Count == 0)
        {
            AbilityFeedback.Show("Select Attackers first");
            return;
        }

        if (!HasSelectedAttackers())
        {
            AbilityFeedback.Show("Select Attackers for Charge (Q)");
            return;
        }

        if (abilityTargetingMode == AbilityTargetingMode.Charge)
        {
            CancelAbilityTargeting();
            AbilityFeedback.Show("Charge cancelled");
            return;
        }

        CancelAbilityTargeting();
        abilityTargetingMode = AbilityTargetingMode.Charge;
        AbilityFeedback.Show("Click enemy or ground to charge", 4f);
    }

    void TryExecutePendingAbilityClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length == 0)
            return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        if (abilityTargetingMode == AbilityTargetingMode.Charge)
        {
            Health enemyTarget = FindClosestEnemy(hits);
            if (enemyTarget != null)
            {
                ExecuteCharge(enemyTarget.transform.position, enemyTarget);
                CancelAbilityTargeting();
                return;
            }

            if (TryFindGroundPoint(hits, out Vector3 groundPoint))
            {
                ExecuteCharge(groundPoint, null);
                CancelAbilityTargeting();
            }

            return;
        }

        if (abilityTargetingMode == AbilityTargetingMode.Volley)
        {
            Health enemyTarget = FindClosestEnemy(hits);
            if (enemyTarget != null)
            {
                ExecuteVolley(enemyTarget.transform.position);
                CancelAbilityTargeting();
                return;
            }

            if (TryFindGroundPoint(hits, out Vector3 groundPoint))
            {
                ExecuteVolley(groundPoint);
                CancelAbilityTargeting();
            }
        }
    }

    void ExecuteCharge(Vector3 destination, Health enemyTarget)
    {
        int activated = 0;
        int onCooldown = 0;

        foreach (UnitSelection unit in selectedUnits)
        {
            SquadAbility ability = unit.GetComponent<SquadAbility>();
            Unit unitProfile = unit.GetComponent<Unit>();
            if (ability == null || unitProfile == null || unitProfile.Type != UnitType.Attacker)
                continue;

            if (ability.TryActivateCharge(destination, enemyTarget))
            {
                activated++;
                continue;
            }

            if (ability.GetSnapshot().CooldownRemaining > 0f || ability.IsCharging)
                onCooldown++;
        }

        if (activated > 0)
            AbilityFeedback.Show("Charge!");
        else if (onCooldown > 0)
            AbilityFeedback.Show("Charge on cooldown");
    }

    void CancelAbilityTargeting()
    {
        abilityTargetingMode = AbilityTargetingMode.None;
    }

    bool HasSelectedAttackers()
    {
        foreach (UnitSelection unit in selectedUnits)
        {
            Unit unitProfile = unit.GetComponent<Unit>();
            if (unitProfile != null && unitProfile.Type == UnitType.Attacker)
                return true;
        }

        return false;
    }

    void ActivateVolley()
    {
        PruneDestroyedUnits();
        if (selectedUnits.Count == 0)
        {
            AbilityFeedback.Show("Select Archers first");
            return;
        }

        if (!HasSelectedArchers())
        {
            AbilityFeedback.Show("Select Archers for Volley (W)");
            return;
        }

        if (abilityTargetingMode == AbilityTargetingMode.Volley)
        {
            CancelAbilityTargeting();
            AbilityFeedback.Show("Volley cancelled");
            return;
        }

        CancelAbilityTargeting();
        abilityTargetingMode = AbilityTargetingMode.Volley;
        AbilityFeedback.Show("Click ground or enemy to volley", 4f);
    }

    void ExecuteVolley(Vector3 volleyCenter)
    {
        int activated = 0;
        int onCooldown = 0;

        foreach (UnitSelection unit in selectedUnits)
        {
            SquadAbility ability = unit.GetComponent<SquadAbility>();
            Unit unitProfile = unit.GetComponent<Unit>();
            if (ability == null || unitProfile == null || unitProfile.Type != UnitType.Archer)
                continue;

            if (ability.TryActivateVolley(volleyCenter))
            {
                activated++;
                continue;
            }

            if (ability.GetSnapshot().CooldownRemaining > 0f)
                onCooldown++;
        }

        if (activated > 0)
            AbilityFeedback.Show("Volley!");
        else if (onCooldown > 0)
            AbilityFeedback.Show("Volley on cooldown");
    }

    bool HasSelectedArchers()
    {
        foreach (UnitSelection unit in selectedUnits)
        {
            Unit unitProfile = unit.GetComponent<Unit>();
            if (unitProfile != null && unitProfile.Type == UnitType.Archer)
                return true;
        }

        return false;
    }

    void AttackWithSelectedUnits(Health target)
    {
        PruneDestroyedUnits();
        HashSet<UnitType> focusedSquads = new HashSet<UnitType>();

        foreach (UnitSelection unit in selectedUnits)
        {
            PlayerUnitAI ai = unit.GetComponent<PlayerUnitAI>();
            if (ai != null)
                ai.AttackTarget(target);

            Unit unitProfile = unit.GetComponent<Unit>();
            if (unitProfile != null)
                focusedSquads.Add(unitProfile.Type);
        }

        foreach (UnitType squadType in focusedSquads)
            FocusFireRegistry.SetFocus(squadType, target);

        AbilityFeedback.Show($"Focus fire: {UnitIdentity.GetLabel(target)}", 2.5f);
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
        Unit unitProfile = unit.GetComponent<Unit>();
        if (unitProfile != null)
            FocusFireRegistry.SetFocus(unitProfile.Type, null);

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
        CancelAbilityTargeting();
        PruneDestroyedUnits();

        foreach (UnitSelection unit in selectedUnits)
            unit.Deselect();

        selectedUnits.Clear();
    }
}
