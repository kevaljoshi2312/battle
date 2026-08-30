using UnityEngine;

public static class UnitNavigation
{
    public static Vector3 GetSurroundChasePosition(
        Vector3 from,
        Vector3 targetPosition,
        float stopDistance,
        int slotSeed)
    {
        Vector3 toTarget = targetPosition - from;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance <= stopDistance || distance <= Mathf.Epsilon)
            return from;

        Vector3 forward = toTarget / distance;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        int slotIndex = Mod(slotSeed, UnitVisuals.ApproachSlotCount);
        float centerOffset = (UnitVisuals.ApproachSlotCount - 1) * 0.5f;
        float lateralOffset = (slotIndex - centerOffset) * UnitVisuals.ApproachSlotSpacing;

        Vector3 destination = targetPosition - forward * stopDistance + right * lateralOffset;
        destination.y = from.y;
        return destination;
    }

    static int Mod(int value, int count)
    {
        int result = value % count;
        return result < 0 ? result + count : result;
    }
}
