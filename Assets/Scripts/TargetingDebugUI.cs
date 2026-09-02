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
        if (!BattleDebug.ShowTargetingUI)
            return;

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
            return "Select a squad (Defenders or Archers) to see targeting info.";

        StringBuilder builder = new StringBuilder();
        IReadOnlyList<UnitType> selectedSquads = controller.SelectedSquads;

        for (int i = 0; i < selectedSquads.Count; i++)
        {
            if (i > 0)
                builder.AppendLine();

            AppendSquadTargeting(builder, selectedSquads[i], controller.SelectedUnits);
        }

        return builder.ToString();
    }

    void AppendSquadTargeting(StringBuilder builder, UnitType squadType, IReadOnlyList<UnitSelection> selectedUnits)
    {
        List<Health> squadMembers = new List<Health>();
        List<string> memberLabels = new List<string>();

        foreach (UnitSelection selection in selectedUnits)
        {
            Unit unit = selection.GetComponent<Unit>();
            if (unit == null || unit.Type != squadType)
                continue;

            Health health = selection.GetComponent<Health>();
            if (health == null || !health.IsAlive)
                continue;

            squadMembers.Add(health);
            memberLabels.Add(UnitIdentity.GetLabel(health));
        }

        memberLabels.Sort();
        builder.AppendLine($"{squadType}s ({squadMembers.Count}): {string.Join(", ", memberLabels)}");

        if (squadMembers.Count == 0)
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

            Health primary = enemy.PrimaryTarget;
            if (primary != null && squadMembers.Contains(primary))
                primaryEnemies.Add(GetEnemyLabel(enemy));

            Health current = enemy.CurrentTarget;
            if (current != null && squadMembers.Contains(current) && current != primary)
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
