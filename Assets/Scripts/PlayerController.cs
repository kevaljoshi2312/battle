using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum SquadTargetingMode
    {
        None,
        Move,
        Attack,
        Charge,
        Volley
    }

    readonly List<UnitSelection> selectedUnits = new List<UnitSelection>();
    SquadTargetingMode targetingMode = SquadTargetingMode.None;
    UnitType? targetingSquad;

    public IReadOnlyList<UnitSelection> SelectedUnits => selectedUnits;
    public SquadTargetingMode TargetingMode => targetingMode;
    public UnitType? TargetingSquad => targetingSquad;
    public bool IsTargetingPending => targetingMode != SquadTargetingMode.None;
    public bool IsMoveTargetingPending => targetingMode == SquadTargetingMode.Move;
    public bool IsAttackTargetingPending => targetingMode == SquadTargetingMode.Attack;
    public bool IsChargeTargetingPending => targetingMode == SquadTargetingMode.Charge;
    public bool IsVolleyTargetingPending => targetingMode == SquadTargetingMode.Volley;

    public IReadOnlyList<UnitType> SelectedSquads => GetSelectedSquadTypes();

    void Update()
    {
        PruneDestroyedUnits();

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            CancelTargeting();
            ActivateShieldWall();
            return;
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            ActivateCharge();
            return;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
            ActivateVolley();
    }

    void LateUpdate()
    {
        if (Mouse.current == null || Camera.main == null)
            return;

        if (IsPointerOverUI())
            return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelTargeting();
            DeselectAll();
            return;
        }

        if (targetingMode != SquadTargetingMode.None &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryExecutePendingTargetClick();
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
            CancelTargeting();
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

    static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }

    public void SelectSquad(UnitType type)
    {
        SelectSquadOnly(type);
    }

    public void SetSquadMode(UnitType type, SquadCommandMode mode)
    {
        SelectSquadOnly(type);
        SquadCommandState.SetMode(type, mode);
        AbilityFeedback.Show($"{GetSquadShortName(type)}: {GetModeLabel(mode)}", 2f);
    }

    public void BeginMoveTargeting(UnitType type)
    {
        SelectSquadOnly(type);
        if (GetLivingPlayerUnitsOfType(type).Count == 0)
        {
            AbilityFeedback.Show($"{GetSquadShortName(type)} squad eliminated");
            return;
        }

        if (targetingMode == SquadTargetingMode.Move && targetingSquad == type)
        {
            CancelTargeting();
            AbilityFeedback.Show("Move cancelled");
            return;
        }

        BeginTargeting(SquadTargetingMode.Move, type, "Click ground to move");
    }

    public void BeginAttackTargeting(UnitType type)
    {
        SelectSquadOnly(type);
        if (GetLivingPlayerUnitsOfType(type).Count == 0)
        {
            AbilityFeedback.Show($"{GetSquadShortName(type)} squad eliminated");
            return;
        }

        if (targetingMode == SquadTargetingMode.Attack && targetingSquad == type)
        {
            CancelTargeting();
            AbilityFeedback.Show("Attack cancelled");
            return;
        }

        BeginTargeting(SquadTargetingMode.Attack, type, "Click enemy to attack");
    }

    public void BeginChargeTargeting(UnitType type)
    {
        if (type != UnitType.Attacker)
            return;

        SelectSquadOnly(type);
        if (GetLivingPlayerUnitsOfType(type).Count == 0)
        {
            AbilityFeedback.Show("Attackers eliminated");
            return;
        }

        if (targetingMode == SquadTargetingMode.Charge && targetingSquad == type)
        {
            CancelTargeting();
            AbilityFeedback.Show("Charge cancelled");
            return;
        }

        BeginTargeting(SquadTargetingMode.Charge, type, "Click enemy or ground to charge");
    }

    public void BeginVolleyTargeting(UnitType type)
    {
        if (type != UnitType.Archer)
            return;

        SelectSquadOnly(type);
        if (GetLivingPlayerUnitsOfType(type).Count == 0)
        {
            AbilityFeedback.Show("Archers eliminated");
            return;
        }

        if (targetingMode == SquadTargetingMode.Volley && targetingSquad == type)
        {
            CancelTargeting();
            AbilityFeedback.Show("Volley cancelled");
            return;
        }

        BeginTargeting(SquadTargetingMode.Volley, type, "Click ground or enemy to volley");
    }

    public void ActivateShieldForSquad(UnitType type)
    {
        SelectSquadOnly(type);
        ActivateShieldWall();
    }

    void BeginTargeting(SquadTargetingMode mode, UnitType squad, string message)
    {
        CancelTargeting();
        targetingMode = mode;
        targetingSquad = squad;
        AbilityFeedback.Show(message, 4f);
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
        HashSet<UnitType> affectedSquads = new HashSet<UnitType>();

        foreach (UnitSelection unit in selectedUnits)
        {
            SquadAbility ability = unit.GetComponent<SquadAbility>();
            Unit unitProfile = unit.GetComponent<Unit>();

            if (ability != null && ability.TryActivateShieldWall())
            {
                activated++;
                if (unitProfile != null)
                    affectedSquads.Add(unitProfile.Type);
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
                if (unitProfile != null)
                    affectedSquads.Add(unitProfile.Type);
            }
        }

        foreach (UnitType squadType in affectedSquads)
            SquadCommandState.SetMode(squadType, SquadCommandMode.Hold);

        if (activated > 0)
            AbilityFeedback.Show("Shield Wall active");
        else if (onCooldown > 0)
            AbilityFeedback.Show("Shield Wall on cooldown");
        else if (held > 0)
            AbilityFeedback.Show("Squads holding position");
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

        BeginChargeTargeting(UnitType.Attacker);
    }

    void TryExecutePendingTargetClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length == 0)
            return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        switch (targetingMode)
        {
            case SquadTargetingMode.Move:
                if (TryFindGroundPoint(hits, out Vector3 movePoint))
                {
                    MoveSelectedUnitsTo(movePoint);
                    CancelTargeting();
                }

                return;

            case SquadTargetingMode.Attack:
            {
                Health attackTarget = FindClosestEnemy(hits);
                if (attackTarget != null)
                {
                    AttackWithSelectedUnits(attackTarget);
                    CancelTargeting();
                }

                return;
            }

            case SquadTargetingMode.Charge:
            {
                Health enemyTarget = FindClosestEnemy(hits);
                if (enemyTarget != null)
                {
                    ExecuteCharge(enemyTarget.transform.position, enemyTarget);
                    CancelTargeting();
                    return;
                }

                if (TryFindGroundPoint(hits, out Vector3 chargePoint))
                {
                    ExecuteCharge(chargePoint, null);
                    CancelTargeting();
                }

                return;
            }

            case SquadTargetingMode.Volley:
            {
                Health volleyTarget = FindClosestEnemy(hits);
                if (volleyTarget != null)
                {
                    ExecuteVolley(volleyTarget.transform.position);
                    CancelTargeting();
                    return;
                }

                if (TryFindGroundPoint(hits, out Vector3 volleyPoint))
                {
                    ExecuteVolley(volleyPoint);
                    CancelTargeting();
                }

                return;
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

    public void CancelTargeting()
    {
        targetingMode = SquadTargetingMode.None;
        targetingSquad = null;
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

        BeginVolleyTargeting(UnitType.Archer);
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
        CancelTargeting();
        PruneDestroyedUnits();

        foreach (UnitSelection unit in selectedUnits)
            unit.Deselect();

        selectedUnits.Clear();
    }

    static string GetSquadShortName(UnitType type)
    {
        return type switch
        {
            UnitType.Defender => "DEF",
            UnitType.Attacker => "ATK",
            UnitType.Archer => "ARC",
            _ => type.ToString().ToUpper()
        };
    }

    static string GetModeLabel(SquadCommandMode mode)
    {
        return mode switch
        {
            SquadCommandMode.Hold => "Hold",
            SquadCommandMode.Manual => "Manual",
            _ => "Auto"
        };
    }
}
