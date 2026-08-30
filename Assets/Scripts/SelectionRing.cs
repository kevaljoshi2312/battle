using UnityEngine;

public class SelectionRing : MonoBehaviour
{
    const string RingName = "SelectionRing";

    [SerializeField] Color ringColor = new Color(0.2f, 0.9f, 0.35f);

    GameObject ringObject;

    void Awake()
    {
        BuildRing();
        Hide();
    }

    public void Show()
    {
        if (ringObject != null)
            ringObject.SetActive(true);
    }

    public void Hide()
    {
        if (ringObject != null)
            ringObject.SetActive(false);
    }

    void BuildRing()
    {
        Transform existing = transform.Find(RingName);
        if (existing != null)
            Destroy(existing.gameObject);

        ringObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ringObject.name = RingName;
        ringObject.transform.SetParent(transform, false);

        float diameter = UnitVisuals.SelectionRingDiameter;
        float halfHeight = UnitVisuals.CapsuleHeight * 0.5f;
        ringObject.transform.localScale = new Vector3(diameter, UnitVisuals.SelectionRingHeight, diameter);
        ringObject.transform.localPosition = new Vector3(0f, -halfHeight + 0.02f, 0f);

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
        material.color = ringColor;
        renderer.material = material;
    }
}
