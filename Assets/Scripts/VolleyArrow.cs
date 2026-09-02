using UnityEngine;

public class VolleyArrow : MonoBehaviour
{
    Vector3 targetPosition;
    float speed;

    public void Launch(Vector3 impactPoint, float travelSpeed)
    {
        targetPosition = impactPoint + Vector3.up * UnitVisuals.ArrowTargetHeight;
        speed = travelSpeed;
        BuildVisual();
    }

    void Update()
    {
        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;

        if (distance <= UnitVisuals.ArrowHitDistance)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = toTarget / distance;
        transform.position += direction * (speed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(direction);
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
