using UnityEngine;

public class VolleyArrow : MonoBehaviour
{
    Vector3 startPosition;
    Vector3 endPosition;
    float speed;
    float flightProgress;

    public void Launch(Vector3 launchPosition, Vector3 impactPoint, float travelSpeed)
    {
        startPosition = launchPosition;
        endPosition = impactPoint + Vector3.up * UnitVisuals.ArrowTargetHeight;
        speed = travelSpeed;
        flightProgress = 0f;

        transform.position = launchPosition;
        ArrowVisual.BuildProjectile(transform);
        UpdateFlight(0f);
    }

    void Update()
    {
        float horizontalDistance = ArrowTrajectory.GetHorizontalDistance(startPosition, endPosition);
        float duration = ArrowTrajectory.GetFlightDuration(horizontalDistance, speed);
        flightProgress += Time.deltaTime / duration;

        if (flightProgress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        UpdateFlight(flightProgress);
    }

    void UpdateFlight(float progress)
    {
        float arcHeight = ArrowTrajectory.GetArcHeight(
            ArrowTrajectory.GetHorizontalDistance(startPosition, endPosition));

        transform.position = ArrowTrajectory.GetPoint(startPosition, endPosition, progress, arcHeight);
        transform.rotation = Quaternion.LookRotation(
            ArrowTrajectory.GetTangent(startPosition, endPosition, progress, arcHeight),
            Vector3.up);
    }
}
