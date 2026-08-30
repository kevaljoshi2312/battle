using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    GameObject resultPanel;
    Text resultText;
    bool battleEnded;

    void Awake()
    {
        BuildUI();
    }

    void Update()
    {
        if (battleEnded)
            return;

        if (!IsTeamAlive(Team.Player))
            EndBattle("Defeat");
        else if (!IsTeamAlive(Team.Enemy))
            EndBattle("Victory");
    }

    static bool IsTeamAlive(Team team)
    {
        foreach (Health health in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (!health.IsAlive)
                continue;

            UnitTeam unitTeam = health.GetComponent<UnitTeam>();
            if (unitTeam != null && unitTeam.Team == team)
                return true;
        }

        return false;
    }

    void EndBattle(string result)
    {
        battleEnded = true;
        CleanupDyingUnits();
        Time.timeScale = 0f;
        resultPanel.SetActive(true);
        resultText.text = result;
    }

    static void CleanupDyingUnits()
    {
        foreach (Health health in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (health.IsAlive)
                continue;

            DeathEffect deathEffect = health.GetComponent<DeathEffect>();
            if (deathEffect != null)
                deathEffect.FinishImmediately();
            else
                Destroy(health.gameObject);
        }
    }

    public void RestartBattle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void BuildUI()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("BattleUI");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        resultPanel = new GameObject("ResultPanel");
        resultPanel.transform.SetParent(canvasObject.transform, false);

        Image panelImage = resultPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.65f);

        RectTransform panelRect = resultPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("ResultText");
        textObject.transform.SetParent(resultPanel.transform, false);

        resultText = textObject.AddComponent<Text>();
        resultText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        resultText.fontSize = 48;
        resultText.alignment = TextAnchor.MiddleCenter;
        resultText.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.55f);
        textRect.anchorMax = new Vector2(0.5f, 0.55f);
        textRect.sizeDelta = new Vector2(500f, 80f);

        GameObject buttonObject = new GameObject("RestartButton");
        buttonObject.transform.SetParent(resultPanel.transform, false);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.45f, 0.85f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(RestartBattle);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.sizeDelta = new Vector2(200f, 50f);

        GameObject buttonLabelObject = new GameObject("Label");
        buttonLabelObject.transform.SetParent(buttonObject.transform, false);

        Text buttonLabel = buttonLabelObject.AddComponent<Text>();
        buttonLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonLabel.fontSize = 24;
        buttonLabel.alignment = TextAnchor.MiddleCenter;
        buttonLabel.color = Color.white;
        buttonLabel.text = "Restart";

        RectTransform buttonLabelRect = buttonLabelObject.GetComponent<RectTransform>();
        buttonLabelRect.anchorMin = Vector2.zero;
        buttonLabelRect.anchorMax = Vector2.one;
        buttonLabelRect.offsetMin = Vector2.zero;
        buttonLabelRect.offsetMax = Vector2.zero;

        resultPanel.SetActive(false);
    }

    static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }
}
