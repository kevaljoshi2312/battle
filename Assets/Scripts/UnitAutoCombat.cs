using UnityEngine;

public static class UnitAutoCombat
{
    public static Health ResolveTarget(
        Vector3 from,
        GameObject self,
        UnitTeam unitTeam,
        float detectRange,
        Health preferredTarget)
    {
        if (unitTeam == null)
            return null;

        Health target = preferredTarget;
        if (target == null || !target.IsAlive)
        {
            target = UnitNavigation.FindClosestOpponent(
                from,
                unitTeam.Team,
                self,
                detectRange);
        }

        if (target == null || !target.IsAlive)
            return null;

        Health blocker = UnitNavigation.FindBlockingOpponent(
            from,
            target,
            unitTeam.Team,
            self);

        return blocker != null ? blocker : target;
    }

    public static Health Tick(
        GameObject self,
        UnitMovement movement,
        UnitCombat combat,
        UnitFacing facing,
        Health target,
        bool useSurroundSlots,
        int slotSeed,
        ref Vector3 lastMoveDestination,
        ref float lastMoveOrderTime)
    {
        if (combat == null)
            return null;

        if (target == null || !target.IsAlive)
        {
            movement?.Stop();
            combat.ClearAttackTarget();
            return null;
        }

        combat.SetAttackTarget(target);

        Transform targetTransform = target.transform;
        facing?.FaceToward(targetTransform.position);

        if (combat.IsInRange(targetTransform))
        {
            movement?.Stop();
            combat.TryAttack(target);
            return target;
        }

        float stopDistance = UnitVisuals.GetChaseStopDistance(combat.AttackRange);
        Vector3 chasePosition = useSurroundSlots
            ? UnitNavigation.GetSurroundChasePosition(
                self.transform.position,
                targetTransform.position,
                stopDistance,
                combat.AttackRange,
                slotSeed)
            : UnitNavigation.GetDirectChasePosition(
                self.transform.position,
                targetTransform.position,
                stopDistance);

        chasePosition = UnitNavigation.ClampToBridgeApproach(chasePosition, self.transform.position);

        if (!UnitNavigation.ShouldIssueMoveOrder(chasePosition, lastMoveDestination, lastMoveOrderTime))
            return target;

        lastMoveDestination = chasePosition;
        lastMoveOrderTime = Time.time;
        movement?.MoveToChase(chasePosition);
        return target;
    }
}
