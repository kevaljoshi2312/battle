using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TargetingDebugUI : MonoBehaviour
{
    Text panelText;
    EnemyAI[] cachedEnemies = System.Array.Empty<EnemyAI>();
    float nextEnemyCacheTime;

    void LateUpdate()
    {
        if (panelText == null)
            BuildPanel();

        if (panelText == null)
            return;

        RefreshEnemyCache();
        panelText.text = BuildPanelText();
    }

    void RefreshEnemyCache()
    {
        if (Time.time < nextEnemyCacheTime)
            return;

        nextEnemyCacheTime = Time.time + 0.25f;
        cachedEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
    }

    string BuildPanelText()
    {
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller == null || controller.SelectedUnits.Count == 0)
            return "Select a player unit to see targeting info.";

        StringBuilder builder = new StringBuilder();
        IReadOnlyList<UnitSelection> selectedUnits = controller.SelectedUnits;

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            UnitSelection selection = selectedUnits[i];
            if (selection == null)
                continue;

            if (i > 0)
                builder.AppendLine();

            AppendUnitTargeting(builder, selection);
        }

        return builder.ToString();
    }

    void AppendUnitTargeting(StringBuilder builder, UnitSelection selection)
    {
        Health health = selection.GetComponent<Health>();
        UnitIdentity identity = selection.GetComponent<UnitIdentity>();
        string unitLabel = identity != null ? $"{selection.gameObject.name} ({identity.Label})" : selection.gameObject.name;

        builder.AppendLine(unitLabel);

        if (health == null || !health.IsAlive)
        {
            builder.AppendLine("  (dead)");
            return;
        }

        List<string> primaryEnemies = new List<string>();
        List<string> currentEnemies = new List<string>();

        foreach (EnemyAI enemy in cachedEnemies)
        {
            if (enemy == null)
                continue;

            if (enemy.PrimaryTarget == health)
                primaryEnemies.Add(GetEnemyLabel(enemy));

            if (enemy.CurrentTarget == health && enemy.CurrentTarget != enemy.PrimaryTarget)
                currentEnemies.Add(GetEnemyLabel(enemy));
        }

        primaryEnemies.Sort();
        currentEnemies.Sort();

        builder.AppendLine($"  Primary targets: {FormatList(primaryEnemies)}");
        builder.AppendLine($"  Fighting now: {FormatList(currentEnemies)}");
    }

    static string GetEnemyLabel(EnemyAI enemy)
    {
        UnitIdentity identity = enemy.GetComponent<UnitIdentity>();
        return identity != null ? identity.Label : enemy.gameObject.name;
    }

    static string FormatList(List<string> items)
    {
        if (items.Count == 0)
            return "none";

        return string.Join(", ", items);
    }

    void BuildPanel()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        GameObject panelObject = new GameObject("TargetingDebugPanel");
        panelObject.transform.SetParent(canvas.transform, false);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.55f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(12f, -12f);
        panelRect.sizeDelta = new Vector2(360f, 220f);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(panelObject.transform, false);

        panelText = textObject.AddComponent<Text>();
        panelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        panelText.fontSize = 16;
        panelText.alignment = TextAnchor.UpperLeft;
        panelText.color = Color.white;
        panelText.horizontalOverflow = HorizontalWrapMode.Wrap;
        panelText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 10f);
        textRect.offsetMax = new Vector2(-10f, -10f);
    }
}
