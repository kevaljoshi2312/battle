using UnityEngine;
using UnityEngine.Rendering;

public class HealthBar : MonoBehaviour
{
    [SerializeField] float barWidth = UnitVisuals.HealthBarWidth;
    [SerializeField] float barHeight = UnitVisuals.HealthBarHeight;
    [SerializeField] float heightPadding = 0.2f;

    Health health;
    Transform barRoot;
    Transform fillTransform;
    float cachedPercent = -1f;
    string barRootName;

    void Awake()
    {
        health = GetComponent<Health>();
        barWidth = UnitVisuals.HealthBarWidth;
        barHeight = UnitVisuals.HealthBarHeight;
        barRootName = $"{gameObject.name}_HealthBar";
        CleanupExistingBar();
        BuildBar();
    }

    void LateUpdate()
    {
        if (health == null || barRoot == null)
            return;

        barRoot.position = GetBarWorldPosition();
        FaceCamera();

        float percent = health.MaxHealth > 0
            ? (float)health.CurrentHealth / health.MaxHealth
            : 0f;

        if (Mathf.Approximately(percent, cachedPercent))
            return;

        cachedPercent = percent;
        UpdateFill(percent);
    }

    void OnDestroy()
    {
        if (barRoot != null)
            Destroy(barRoot.gameObject);
    }

    void FaceCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        // Match the fixed RTS camera so every bar has the same roll (not per-position billboarding).
        barRoot.rotation = camera.transform.rotation;
    }

    void CleanupExistingBar()
    {
        Transform legacyCanvas = transform.Find("HealthBar");
        if (legacyCanvas != null)
            Destroy(legacyCanvas.gameObject);

        Transform legacyRoot = transform.Find("HealthBarRoot");
        if (legacyRoot != null)
            Destroy(legacyRoot.gameObject);

        GameObject orphaned = GameObject.Find(barRootName);
        if (orphaned != null)
            Destroy(orphaned);
    }

    void BuildBar()
    {
        GameObject root = new GameObject(barRootName);
        root.transform.SetParent(null);
        barRoot = root.transform;

        CreateQuad("Background", barRoot, new Color(0.15f, 0.15f, 0.15f, 0.9f),
            Vector3.zero, new Vector3(barWidth, barHeight, 1f));

        GameObject fill = CreateQuad("Fill", barRoot, GetFillColor(),
            Vector3.zero, new Vector3(barWidth, barHeight * 0.7f, 1f));
        fillTransform = fill.transform;

        cachedPercent = -1f;
        UpdateFill(1f);
    }

    static GameObject CreateQuad(string name, Transform parent, Color color, Vector3 localPos, Vector3 localScale)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        quad.transform.SetParent(parent, false);
        quad.transform.localPosition = localPos;
        quad.transform.localScale = localScale;

        Collider collider = quad.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        Renderer renderer = quad.GetComponent<Renderer>();
        if (renderer == null)
            return quad;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material material = new Material(shader);
        material.color = color;
        material.SetInt("_Cull", (int)CullMode.Off);
        renderer.material = material;

        return quad;
    }

    void UpdateFill(float percent)
    {
        if (fillTransform == null)
            return;

        float clamped = Mathf.Clamp01(percent);
        float fillWidth = barWidth * clamped;

        fillTransform.localScale = new Vector3(fillWidth, barHeight * 0.7f, 1f);
        fillTransform.localPosition = new Vector3(
            -barWidth * 0.5f + fillWidth * 0.5f,
            0f,
            -0.01f);
    }

    Vector3 GetBarWorldPosition()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null)
            return transform.position + Vector3.up * 1.2f;

        return capsule.bounds.center + Vector3.up * (capsule.bounds.extents.y + heightPadding);
    }

    Color GetFillColor()
    {
        UnitTeam unitTeam = GetComponent<UnitTeam>();
        if (unitTeam == null)
            return Color.green;

        return unitTeam.Team == Team.Player
            ? new Color(0.2f, 0.85f, 0.3f)
            : new Color(0.9f, 0.25f, 0.25f);
    }
}
