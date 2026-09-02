using UnityEngine;

public class UnitWorldLabel : MonoBehaviour
{
    const float LabelHeightPadding = 0.42f;
    const float CharacterSize = 0.08f;

    Health health;
    UnitIdentity identity;
    EnemyAI enemyAI;
    Transform labelRoot;
    TextMesh textMesh;
    string cachedText;

    void Awake()
    {
        if (!BattleDebug.ShowWorldLabels)
            return;

        health = GetComponent<Health>();
        identity = GetComponent<UnitIdentity>();
        enemyAI = GetComponent<EnemyAI>();
        BuildLabel();
    }

    public void Hide()
    {
        if (labelRoot != null)
            labelRoot.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (!BattleDebug.ShowWorldLabels)
            return;

        if (health == null || labelRoot == null || textMesh == null)
            return;

        if (!health.IsAlive)
        {
            Hide();
            return;
        }

        labelRoot.position = GetLabelWorldPosition();
        FaceCamera();

        string label = BuildLabelText();
        if (label == cachedText)
            return;

        cachedText = label;
        textMesh.text = label;
    }

    void OnDestroy()
    {
        if (labelRoot != null)
            Destroy(labelRoot.gameObject);
    }

    string BuildLabelText()
    {
        if (identity == null)
            return gameObject.name;

        if (enemyAI == null)
            return identity.Label;

        Health primary = enemyAI.PrimaryTarget;
        if (primary == null)
            return identity.Label;

        string text = $"{identity.Label} -> {UnitIdentity.GetLabel(primary)}";
        Health current = enemyAI.CurrentTarget;
        if (current != null && current != primary)
            text += $" ({UnitIdentity.GetLabel(current)})";

        return text;
    }

    void BuildLabel()
    {
        string rootName = $"{gameObject.name}_WorldLabel";
        GameObject orphaned = GameObject.Find(rootName);
        if (orphaned != null)
            Destroy(orphaned);

        GameObject root = new GameObject(rootName);
        root.transform.SetParent(null);
        labelRoot = root.transform;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(labelRoot, false);
        textObject.transform.localPosition = Vector3.zero;

        textMesh = textObject.AddComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = CharacterSize;
        textMesh.fontSize = 64;
        textMesh.color = Color.white;
        textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        cachedText = string.Empty;
    }

    void FaceCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        labelRoot.rotation = camera.transform.rotation;
    }

    Vector3 GetLabelWorldPosition()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null)
            return transform.position + Vector3.up * 1.45f;

        return capsule.bounds.center + Vector3.up * (capsule.bounds.extents.y + LabelHeightPadding);
    }
}
