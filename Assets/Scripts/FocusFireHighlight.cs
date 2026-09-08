using UnityEngine;

public class FocusFireHighlight : MonoBehaviour
{
    const string RingName = "FocusFireRing";

    GameObject ringObject;

    void Awake()
    {
        BuildRing();
        SetActive(false);
    }

    public void SetActive(bool visible)
    {
        if (ringObject != null)
            ringObject.SetActive(visible);
    }

    void BuildRing()
    {
        Transform existing = transform.Find(RingName);
        if (existing != null)
            Destroy(existing.gameObject);

        ringObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ringObject.name = RingName;
        ringObject.transform.SetParent(transform, false);

        float diameter = UnitVisuals.SelectionRingDiameter * 1.15f;
        float halfHeight = UnitVisuals.CapsuleHeight * 0.5f;
        ringObject.transform.localScale = new Vector3(diameter, UnitVisuals.SelectionRingHeight * 1.2f, diameter);
        ringObject.transform.localPosition = new Vector3(0f, -halfHeight + 0.03f, 0f);

        Collider collider = ringObject.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        Renderer renderer = ringObject.GetComponent<Renderer>();
        if (renderer == null)
            return;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material material = new Material(shader);
        material.color = new Color(1f, 0.55f, 0.1f, 0.9f);
        renderer.sharedMaterial = material;
    }
}
