using UnityEngine;
using UnityEngine.UI;

public class BattleWarningUI : MonoBehaviour
{
    Text warningText;
    float hideAtTime;

    void LateUpdate()
    {
        if (warningText == null || hideAtTime <= 0f)
            return;

        if (Time.time >= hideAtTime)
        {
            warningText.text = string.Empty;
            hideAtTime = 0f;
        }
    }

    public void Show(string message, float durationSeconds = 4.5f)
    {
        if (warningText == null)
            BuildWarningBanner();

        if (warningText == null)
            return;

        warningText.text = message;
        hideAtTime = Time.time + durationSeconds;
    }

    void BuildWarningBanner()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        GameObject bannerObject = new GameObject("BattleWarningBanner");
        bannerObject.transform.SetParent(canvas.transform, false);

        Image background = bannerObject.AddComponent<Image>();
        background.color = new Color(0.15f, 0.05f, 0.05f, 0.82f);

        RectTransform bannerRect = bannerObject.GetComponent<RectTransform>();
        bannerRect.anchorMin = new Vector2(0.5f, 1f);
        bannerRect.anchorMax = new Vector2(0.5f, 1f);
        bannerRect.pivot = new Vector2(0.5f, 1f);
        bannerRect.anchoredPosition = new Vector2(0f, -12f);
        bannerRect.sizeDelta = new Vector2(520f, 42f);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(bannerObject.transform, false);

        warningText = textObject.AddComponent<Text>();
        warningText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        warningText.fontSize = 20;
        warningText.fontStyle = FontStyle.Bold;
        warningText.alignment = TextAnchor.MiddleCenter;
        warningText.color = new Color(1f, 0.85f, 0.25f);
    }
}
