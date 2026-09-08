using UnityEngine;

public static class UnitNavigation
{
    public static Vector3 GetSurroundChasePosition(
        Vector3 from,
        Vector3 targetPosition,
        float stopDistance,
        float attackRange,
        int slotSeed)
    {
        Vector3 toTarget = targetPosition - from;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance <= Mathf.Epsilon)
            return from;

        Vector3 forward = toTarget / distance;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        int slotIndex = Mod(slotSeed, UnitVisuals.ApproachSlotCount);
        float centerOffset = (UnitVisuals.ApproachSlotCount - 1) * 0.5f;
        float lateralOffset = (slotIndex - centerOffset) * UnitVisuals.ApproachSlotSpacing;

        if (attackRange > stopDistance)
        {
            float maxLateral = Mathf.Sqrt(attackRange * attackRange - stopDistance * stopDistance) * 0.95f;
            lateralOffset = Mathf.Clamp(lateralOffset, -maxLateral, maxLateral);
        }

        float holdDistance = Mathf.Min(stopDistance, distance - 0.1f);
        Vector3 destination = targetPosition - forward * holdDistance + right * lateralOffset;
        destination.y = from.y;
        return destination;
    }

    public static bool ShouldIssueMoveOrder(
        Vector3 desiredDestination,
        Vector3 lastDestination,
        float lastOrderTime)
    {
        if (Time.time - lastOrderTime < UnitVisuals.MoveOrderInterval)
        {
            Vector3 delta = desiredDestination - lastDestination;
            delta.y = 0f;
            if (delta.sqrMagnitude < UnitVisuals.MoveDestinationEpsilon * UnitVisuals.MoveDestinationEpsilon)
                return false;
        }

        return true;
    }

    public static int Mod(int value, int count)
    {
        int result = value % count;
        return result < 0 ? result + count : result;
    }

    static float HorizontalDistance(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        delta.y = 0f;
        return delta.magnitude;
    }

    public static Health FindClosestOpponent(
        Vector3 from,
        Team myTeam,
        GameObject exclude,
        float maxRange)
    {
        Health closest = null;
        float closestDistanceSq = maxRange * maxRange;

        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (!health.IsAlive || health.gameObject == exclude)
                continue;

            UnitTeam otherTeam = health.GetComponent<UnitTeam>();
            if (otherTeam == null || otherTeam.Team == myTeam)
                continue;

            Vector3 delta = health.transform.position - from;
            delta.y = 0f;
            float distanceSq = delta.sqrMagnitude;
            if (distanceSq >= closestDistanceSq)
                continue;

            closest = health;
            closestDistanceSq = distanceSq;
        }

        return closest;
    }

    public static Health FindBlockingOpponent(
        Vector3 from,
        Health focusTarget,
        Team myTeam,
        GameObject exclude)
    {
        if (focusTarget == null || !focusTarget.IsAlive)
            return null;

        Vector3 toFocus = focusTarget.transform.position - from;
        toFocus.y = 0f;
        float focusDistance = toFocus.magnitude;
        if (focusDistance <= Mathf.Epsilon)
            return null;

        Vector3 forward = toFocus / focusDistance;
        float corridorHalfWidth = UnitVisuals.CapsuleWorldRadius * 2f;

        Health closest = null;
        float closestDistance = float.MaxValue;

        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (!health.IsAlive || health.gameObject == exclude || health == focusTarget)
                continue;

            UnitTeam otherTeam = health.GetComponent<UnitTeam>();
            if (otherTeam == null || otherTeam.Team == myTeam)
                continue;

            Vector3 toOpponent = health.transform.position - from;
            toOpponent.y = 0f;
            float distance = toOpponent.magnitude;

            float along = Vector3.Dot(toOpponent, forward);
            if (along <= 0f || along >= focusDistance)
                continue;

            float lateralDistance = (toOpponent - forward * along).magnitude;
            if (lateralDistance > corridorHalfWidth)
                continue;

            if (distance >= closestDistance)
                continue;

            closest = health;
            closestDistance = distance;
        }

        return closest;
    }

    public static Vector3 ClampToBridgeApproach(Vector3 destination, Vector3 from)
    {
        if (!BattlefieldConfig.IsBridgeChokepointEnabled)
            return destination;

        if (from.z <= BattlefieldLayout.PlayerDefenderZ)
            return destination;

        float halfWidth = BattlefieldLayout.BridgeApproachHalfWidth;
        destination.x = Mathf.Clamp(destination.x, -halfWidth, halfWidth);
        return destination;
    }

    public static Vector3 GetDirectChasePosition(Vector3 from, Vector3 targetPosition, float stopDistance)
    {
        Vector3 toTarget = targetPosition - from;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance <= stopDistance || distance <= Mathf.Epsilon)
            return from;

        return targetPosition - toTarget.normalized * stopDistance;
    }
}
