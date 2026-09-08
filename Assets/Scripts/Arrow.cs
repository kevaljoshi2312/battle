using UnityEngine;

public class Arrow : MonoBehaviour
{
    Health target;
    int damage;
    float speed;
    Vector3 startPosition;
    float flightProgress;
    bool reportFlankFeedback;
    Team attackerTeam;

    public void Launch(
        Vector3 launchPosition,
        Health attackTarget,
        int damageAmount,
        float travelSpeed,
        Team sourceTeam = Team.Player,
        bool showFlankFeedback = true)
    {
        target = attackTarget;
        damage = damageAmount;
        speed = travelSpeed;
        startPosition = launchPosition;
        flightProgress = 0f;
        reportFlankFeedback = showFlankFeedback;
        attackerTeam = sourceTeam;

        transform.position = launchPosition;
        ArrowVisual.BuildProjectile(transform);
        UpdateFlight(0f);
    }

    void Update()
    {
        if (target == null || !target.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 endPosition = GetTargetPosition();
        float horizontalDistance = ArrowTrajectory.GetHorizontalDistance(startPosition, endPosition);
        if (horizontalDistance <= UnitVisuals.ArrowHitDistance)
        {
            ApplyDamage();
            Destroy(gameObject);
            return;
        }

        float duration = ArrowTrajectory.GetFlightDuration(horizontalDistance, speed);
        flightProgress += Time.deltaTime / duration;

        if (flightProgress >= 1f)
        {
            ApplyDamage();
            Destroy(gameObject);
            return;
        }

        UpdateFlight(flightProgress);
    }

    void ApplyDamage()
    {
        if (target == null || !target.IsAlive)
            return;

        int finalDamage = FlankingCombat.ApplyFlankingDamage(
            damage,
            GetImpactFlankOrigin(),
            target,
            out FlankType flankType);

        if (reportFlankFeedback && attackerTeam == Team.Player)
        {
            string label = FlankingCombat.GetFlankLabel(flankType);
            if (!string.IsNullOrEmpty(label))
                AbilityFeedback.Show(label, 1.2f);
        }

        target.TakeDamage(finalDamage);
    }

    void UpdateFlight(float progress)
    {
        if (target == null || !target.IsAlive)
            return;

        Vector3 endPosition = GetTargetPosition();
        float arcHeight = ArrowTrajectory.GetArcHeight(
            ArrowTrajectory.GetHorizontalDistance(startPosition, endPosition));

        transform.position = ArrowTrajectory.GetPoint(startPosition, endPosition, progress, arcHeight);
        transform.rotation = Quaternion.LookRotation(
            ArrowTrajectory.GetTangent(startPosition, endPosition, progress, arcHeight),
            Vector3.up);
    }

    Vector3 GetTargetPosition()
    {
        return target.transform.position + Vector3.up * UnitVisuals.ArrowTargetHeight;
    }

    Vector3 GetImpactFlankOrigin()
    {
        Vector3 endPosition = GetTargetPosition();
        Vector3 approachFrom = startPosition - endPosition;
        approachFrom.y = 0f;

        if (approachFrom.sqrMagnitude < 0.001f)
            return endPosition;

        return target.transform.position + approachFrom.normalized;
    }
}
