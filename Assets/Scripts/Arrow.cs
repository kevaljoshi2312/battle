using UnityEngine;

public class Arrow : MonoBehaviour
{
    Health target;
    int damage;
    float speed;
    float hitDistance;

    public void Launch(Vector3 startPosition, Health attackTarget, int damageAmount, float travelSpeed)
    {
        target = attackTarget;
        damage = damageAmount;
        speed = travelSpeed;
        hitDistance = UnitVisuals.ArrowHitDistance;

        transform.position = startPosition;
        BuildVisual();
        UpdateFacing();
    }

    void Update()
    {
        if (target == null || !target.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = GetTargetPosition();
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance <= hitDistance)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Vector3 direction = toTarget.normalized;
        transform.position += direction * (speed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(direction);
    }

    Vector3 GetTargetPosition()
    {
        return target.transform.position + Vector3.up * UnitVisuals.ArrowTargetHeight;
    }

    void UpdateFacing()
    {
        if (target == null)
            return;

        Vector3 toTarget = GetTargetPosition() - transform.position;
        if (toTarget.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(toTarget.normalized);
    }

    void BuildVisual()
    {
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.name = "Shaft";
        shaft.transform.SetParent(transform, false);
        shaft.transform.localScale = UnitVisuals.ArrowScale;
        shaft.transform.localPosition = Vector3.forward * (UnitVisuals.ArrowScale.z * 0.5f);

        Collider collider = shaft.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        Renderer renderer = shaft.GetComponent<Renderer>();
        if (renderer == null)
            return;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material material = new Material(shader);
        material.color = UnitVisuals.ArrowColor;
        renderer.material = material;
    }
}
