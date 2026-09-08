using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityBarUI : MonoBehaviour
{
    struct AbilitySlot
    {
        public UnitType SquadType;
        public string KeyLabel;
        public string AbilityName;
        public float CooldownTotal;
        public Image Background;
        public Image CooldownFill;
        public Text LabelText;
    }

    readonly List<AbilitySlot> slots = new List<AbilitySlot>();
    Text toastText;
    Text focusText;
    Font uiFont;

    void Awake()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildPanel();
    }

    void LateUpdate()
    {
        if (slots.Count == 0)
            return;

        PlayerController controller = FindAnyObjectByType<PlayerController>();
        HashSet<UnitType> selectedTypes = new HashSet<UnitType>();
        if (controller != null)
        {
            foreach (UnitType squadType in controller.SelectedSquads)
                selectedTypes.Add(squadType);
        }

        foreach (AbilitySlot slot in slots)
        {
            SquadAbilitySnapshot snapshot = SquadAbilityState.GetForSquad(slot.SquadType);
            bool isSelected = selectedTypes.Contains(slot.SquadType);
            bool isTargeting = controller != null && (
                (controller.IsChargeTargetingPending && slot.SquadType == UnitType.Attacker) ||
                (controller.IsVolleyTargetingPending && slot.SquadType == UnitType.Archer));
            UpdateSlot(slot, snapshot, isSelected, isTargeting);
        }

        if (toastText != null)
        {
            if (controller != null && controller.IsChargeTargetingPending)
                toastText.text = "Click enemy or ground to charge (Q to cancel)";
            else if (controller != null && controller.IsVolleyTargetingPending)
                toastText.text = "Click ground or enemy to volley (W to cancel)";
            else
                toastText.text = AbilityFeedback.Message;
        }

        if (focusText != null)
            focusText.text = FocusFireRegistry.GetSummary();
    }

    void UpdateSlot(AbilitySlot slot, SquadAbilitySnapshot snapshot, bool isSelected, bool isTargeting)
    {
        Color backgroundColor = new Color(0f, 0f, 0f, isSelected ? 0.7f : 0.45f);
        Color textColor = snapshot.HasLivingUnits ? Color.white : new Color(1f, 1f, 1f, 0.35f);

        if (isSelected && snapshot.HasLivingUnits)
            backgroundColor = new Color(0.1f, 0.25f, 0.5f, 0.85f);

        if (isTargeting)
            backgroundColor = new Color(0.55f, 0.45f, 0.1f, 0.95f);
        else if (snapshot.IsActive)
            backgroundColor = new Color(0.15f, 0.45f, 0.2f, 0.9f);

        slot.Background.color = backgroundColor;
        slot.LabelText.color = textColor;

        string status = isTargeting ? "CLICK TARGET" : BuildStatusText(snapshot);
        slot.LabelText.text = $"[{slot.KeyLabel}] {slot.AbilityName}  {status}";

        if (isTargeting)
        {
            slot.CooldownFill.fillAmount = 1f;
            slot.CooldownFill.color = new Color(0.95f, 0.8f, 0.2f, 0.9f);
            return;
        }

        if (snapshot.IsActive)
        {
            slot.CooldownFill.fillAmount = snapshot.ActiveRemaining / GetActiveDuration(slot.SquadType, snapshot);
            slot.CooldownFill.color = new Color(0.3f, 0.85f, 0.35f, 0.85f);
            return;
        }

        if (snapshot.CooldownRemaining > 0f)
        {
            slot.CooldownFill.fillAmount = snapshot.CooldownRemaining / slot.CooldownTotal;
            slot.CooldownFill.color = new Color(0.85f, 0.35f, 0.2f, 0.85f);
            return;
        }

        slot.CooldownFill.fillAmount = snapshot.IsReady ? 1f : 0f;
        slot.CooldownFill.color = new Color(0.25f, 0.55f, 0.9f, 0.75f);
    }

    static string BuildStatusText(SquadAbilitySnapshot snapshot)
    {
        if (!snapshot.HasLivingUnits)
            return "NO UNITS";

        if (snapshot.IsActive)
            return $"ACTIVE {snapshot.ActiveRemaining:0.#}s";

        if (snapshot.CooldownRemaining > 0f)
            return $"CD {snapshot.CooldownRemaining:0.#}s";

        return "READY";
    }

    static float GetActiveDuration(UnitType squadType, SquadAbilitySnapshot snapshot)
    {
        return squadType switch
        {
            UnitType.Defender => SquadAbility.ShieldWallDurationSeconds,
            UnitType.Attacker => SquadAbility.ChargeDurationSeconds,
            _ => 1f
        };
    }

    void BuildPanel()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        GameObject panelObject = new GameObject("AbilityBarPanel");
        panelObject.transform.SetParent(canvas.transform, false);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.35f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(12f, 12f);
        panelRect.sizeDelta = new Vector2(340f, 136f);

        CreateSlot(panelObject.transform, UnitType.Defender, "H", "Shield Wall", SquadAbility.ShieldWallCooldownSeconds, 86f);
        CreateSlot(panelObject.transform, UnitType.Attacker, "Q", "Charge", SquadAbility.ChargeCooldownSeconds, 58f);
        CreateSlot(panelObject.transform, UnitType.Archer, "W", "Volley", SquadAbility.VolleyCooldownSeconds, 30f);

        GameObject toastObject = new GameObject("AbilityToast");
        toastObject.transform.SetParent(panelObject.transform, false);

        toastText = toastObject.AddComponent<Text>();
        toastText.font = uiFont;
        toastText.fontSize = 14;
        toastText.alignment = TextAnchor.MiddleLeft;
        toastText.color = new Color(1f, 0.85f, 0.35f);

        RectTransform toastRect = toastObject.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0f, 0f);
        toastRect.anchorMax = new Vector2(1f, 0f);
        toastRect.pivot = new Vector2(0f, 0f);
        toastRect.anchoredPosition = new Vector2(8f, 4f);
        toastRect.sizeDelta = new Vector2(-16f, 18f);

        GameObject focusObject = new GameObject("FocusFireLabel");
        focusObject.transform.SetParent(panelObject.transform, false);

        focusText = focusObject.AddComponent<Text>();
        focusText.font = uiFont;
        focusText.fontSize = 13;
        focusText.alignment = TextAnchor.MiddleLeft;
        focusText.color = new Color(1f, 0.7f, 0.35f);

        RectTransform focusRect = focusObject.GetComponent<RectTransform>();
        focusRect.anchorMin = new Vector2(0f, 0f);
        focusRect.anchorMax = new Vector2(1f, 0f);
        focusRect.pivot = new Vector2(0f, 0f);
        focusRect.anchoredPosition = new Vector2(8f, 22f);
        focusRect.sizeDelta = new Vector2(-16f, 16f);
    }

    void CreateSlot(Transform parent, UnitType squadType, string keyLabel, string abilityName, float cooldownTotal, float y)
    {
        GameObject slotObject = new GameObject($"{abilityName}Slot");
        slotObject.transform.SetParent(parent, false);

        Image background = slotObject.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.55f);

        RectTransform slotRect = slotObject.GetComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0f, 1f);
        slotRect.anchorMax = new Vector2(1f, 1f);
        slotRect.pivot = new Vector2(0f, 1f);
        slotRect.anchoredPosition = new Vector2(8f, -y);
        slotRect.sizeDelta = new Vector2(-16f, 24f);

        GameObject fillObject = new GameObject("CooldownFill");
        fillObject.transform.SetParent(slotObject.transform, false);

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = new Color(0.25f, 0.55f, 0.9f, 0.75f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(slotObject.transform, false);

        Text labelText = labelObject.AddComponent<Text>();
        labelText.font = uiFont;
        labelText.fontSize = 14;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 0f);
        labelRect.offsetMax = new Vector2(-8f, 0f);

        slots.Add(new AbilitySlot
        {
            SquadType = squadType,
            KeyLabel = keyLabel,
            AbilityName = abilityName,
            CooldownTotal = cooldownTotal,
            Background = background,
            CooldownFill = fillImage,
            LabelText = labelText
        });
    }
}
