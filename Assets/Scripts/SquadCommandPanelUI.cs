using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquadCommandPanelUI : MonoBehaviour
{
    struct SquadRow
    {
        public UnitType SquadType;
        public string AbilityLabel;
        public string KeyLabel;
        public float CooldownTotal;
        public Image RowBackground;
        public Image IconImage;
        public Text CountText;
        public Text ModeText;
        public Text FocusText;
        public Image AbilityCooldownFill;
        public Button AutoButton;
        public Button HoldButton;
        public Button ManualButton;
        public Button MoveButton;
        public Button AttackButton;
        public Button AbilityButton;
    }

    readonly List<SquadRow> rows = new List<SquadRow>();
    Text toastText;
    Text focusSummaryText;
    Font uiFont;
    PlayerController controller;

    void Awake()
    {
        SquadCommandState.ResetAll();
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildPanel();
    }

    void LateUpdate()
    {
        controller ??= FindAnyObjectByType<PlayerController>();

        HashSet<UnitType> selectedTypes = new HashSet<UnitType>();
        if (controller != null)
        {
            foreach (UnitType squadType in controller.SelectedSquads)
                selectedTypes.Add(squadType);
        }

        foreach (SquadRow row in rows)
            UpdateRow(row, selectedTypes, controller);

        UpdateToast(controller);
        if (focusSummaryText != null)
            focusSummaryText.text = FocusFireRegistry.GetSummary();
    }

    void UpdateToast(PlayerController playerController)
    {
        if (toastText == null)
            return;

        if (playerController == null || !playerController.IsTargetingPending)
        {
            toastText.text = AbilityFeedback.Message;
            return;
        }

        UnitType squad = playerController.TargetingSquad ?? UnitType.Defender;
        toastText.text = playerController.TargetingMode switch
        {
            PlayerController.SquadTargetingMode.Move =>
                $"[{GetSquadShortName(squad)}] Click ground to move (right-click cancel)",
            PlayerController.SquadTargetingMode.Attack =>
                $"[{GetSquadShortName(squad)}] Click enemy to attack (right-click cancel)",
            PlayerController.SquadTargetingMode.Charge =>
                $"[{GetSquadShortName(squad)}] Click enemy or ground to charge (Q/right-click cancel)",
            PlayerController.SquadTargetingMode.Volley =>
                $"[{GetSquadShortName(squad)}] Click ground or enemy to volley (W/right-click cancel)",
            _ => AbilityFeedback.Message
        };
    }

    void UpdateRow(SquadRow row, HashSet<UnitType> selectedTypes, PlayerController playerController)
    {
        SquadRoster.GetCounts(row.SquadType, out int alive, out int max);
        SquadCommandMode mode = SquadCommandState.GetMode(row.SquadType);
        SquadAbilitySnapshot snapshot = SquadAbilityState.GetForSquad(row.SquadType);
        bool isSelected = selectedTypes.Contains(row.SquadType);
        bool isTargeting = playerController != null &&
            playerController.IsTargetingPending &&
            playerController.TargetingSquad == row.SquadType;

        row.CountText.text = $"{alive}/{max}";
        row.ModeText.text = GetModeLabel(mode);
        row.CountText.color = alive > 0 ? Color.white : new Color(1f, 1f, 1f, 0.35f);

        Health focus = FocusFireRegistry.GetFocus(row.SquadType);
        row.FocusText.text = focus != null ? $"→ {UnitIdentity.GetLabel(focus)}" : string.Empty;

        Color rowColor = new Color(0f, 0f, 0f, isSelected ? 0.72f : 0.48f);
        if (isSelected && alive > 0)
            rowColor = new Color(0.1f, 0.25f, 0.5f, 0.88f);
        if (isTargeting)
            rowColor = new Color(0.55f, 0.45f, 0.1f, 0.95f);
        else if (snapshot.IsActive)
            rowColor = new Color(0.15f, 0.45f, 0.2f, 0.9f);

        row.RowBackground.color = rowColor;

        SetButtonHighlight(row.AutoButton, mode == SquadCommandMode.Auto);
        SetButtonHighlight(row.HoldButton, mode == SquadCommandMode.Hold);
        SetButtonHighlight(row.ManualButton, mode == SquadCommandMode.Manual);
        SetButtonHighlight(row.MoveButton, isTargeting && playerController.IsMoveTargetingPending);
        SetButtonHighlight(row.AttackButton, isTargeting && playerController.IsAttackTargetingPending);
        SetButtonHighlight(
            row.AbilityButton,
            isTargeting &&
            (playerController.IsChargeTargetingPending || playerController.IsVolleyTargetingPending));

        UpdateAbilityButton(row, snapshot, isTargeting);
    }

    void UpdateAbilityButton(SquadRow row, SquadAbilitySnapshot snapshot, bool isTargeting)
    {
        bool squadAlive = snapshot.HasLivingUnits;

        if (isTargeting)
        {
            row.AbilityCooldownFill.fillAmount = 1f;
            row.AbilityCooldownFill.color = new Color(0.95f, 0.8f, 0.2f, 0.9f);
            return;
        }

        if (snapshot.IsActive)
        {
            row.AbilityCooldownFill.fillAmount =
                snapshot.ActiveRemaining / GetActiveDuration(row.SquadType);
            row.AbilityCooldownFill.color = new Color(0.3f, 0.85f, 0.35f, 0.85f);
            return;
        }

        if (snapshot.CooldownRemaining > 0f)
        {
            row.AbilityCooldownFill.fillAmount = snapshot.CooldownRemaining / row.CooldownTotal;
            row.AbilityCooldownFill.color = new Color(0.85f, 0.35f, 0.2f, 0.85f);
            return;
        }

        row.AbilityCooldownFill.fillAmount = squadAlive ? 1f : 0f;
        row.AbilityCooldownFill.color = new Color(0.25f, 0.55f, 0.9f, 0.75f);
    }

    static float GetActiveDuration(UnitType squadType)
    {
        return squadType switch
        {
            UnitType.Defender => SquadAbility.ShieldWallDurationSeconds,
            UnitType.Attacker => SquadAbility.ChargeDurationSeconds,
            _ => 1f
        };
    }

    static void SetButtonHighlight(Button button, bool active)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        if (image == null)
            return;

        image.color = active
            ? new Color(0.25f, 0.55f, 0.95f, 0.95f)
            : new Color(0.18f, 0.18f, 0.18f, 0.85f);
    }

    void BuildPanel()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        GameObject panelObject = new GameObject("SquadCommandPanel");
        panelObject.transform.SetParent(canvas.transform, false);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.35f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(12f, 12f);
        panelRect.sizeDelta = new Vector2(620f, 176f);

        CreateRow(panelObject.transform, UnitType.Defender, "SHIELD", "H", SquadAbility.ShieldWallCooldownSeconds, 8f);
        CreateRow(panelObject.transform, UnitType.Attacker, "CHARGE", "Q", SquadAbility.ChargeCooldownSeconds, 44f);
        CreateRow(panelObject.transform, UnitType.Archer, "VOLLEY", "W", SquadAbility.VolleyCooldownSeconds, 80f);

        GameObject toastObject = new GameObject("CommandToast");
        toastObject.transform.SetParent(panelObject.transform, false);

        toastText = toastObject.AddComponent<Text>();
        toastText.font = uiFont;
        toastText.fontSize = 13;
        toastText.alignment = TextAnchor.MiddleLeft;
        toastText.color = new Color(1f, 0.85f, 0.35f);

        RectTransform toastRect = toastObject.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0f, 0f);
        toastRect.anchorMax = new Vector2(1f, 0f);
        toastRect.pivot = new Vector2(0f, 0f);
        toastRect.anchoredPosition = new Vector2(8f, 4f);
        toastRect.sizeDelta = new Vector2(-16f, 18f);

        GameObject focusObject = new GameObject("FocusSummary");
        focusObject.transform.SetParent(panelObject.transform, false);

        focusSummaryText = focusObject.AddComponent<Text>();
        focusSummaryText.font = uiFont;
        focusSummaryText.fontSize = 12;
        focusSummaryText.alignment = TextAnchor.MiddleLeft;
        focusSummaryText.color = new Color(1f, 0.7f, 0.35f);

        RectTransform focusRect = focusObject.GetComponent<RectTransform>();
        focusRect.anchorMin = new Vector2(0f, 0f);
        focusRect.anchorMax = new Vector2(1f, 0f);
        focusRect.pivot = new Vector2(0f, 0f);
        focusRect.anchoredPosition = new Vector2(8f, 22f);
        focusRect.sizeDelta = new Vector2(-16f, 16f);
    }

    void CreateRow(
        Transform parent,
        UnitType squadType,
        string abilityLabel,
        string keyLabel,
        float cooldownTotal,
        float topOffset)
    {
        GameObject rowObject = new GameObject($"{squadType}Row");
        rowObject.transform.SetParent(parent, false);

        Image rowBackground = rowObject.AddComponent<Image>();
        rowBackground.color = new Color(0f, 0f, 0f, 0.55f);
        rowBackground.raycastTarget = false;

        RectTransform rowRect = rowObject.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0f, 1f);
        rowRect.anchoredPosition = new Vector2(8f, -topOffset);
        rowRect.sizeDelta = new Vector2(-16f, 30f);

        GameObject selectObject = new GameObject("SelectArea");
        selectObject.transform.SetParent(rowObject.transform, false);

        Image selectBackground = selectObject.AddComponent<Image>();
        selectBackground.color = new Color(0f, 0f, 0f, 0.01f);

        Button selectButton = selectObject.AddComponent<Button>();
        selectButton.targetGraphic = selectBackground;
        UnitType capturedType = squadType;
        selectButton.onClick.AddListener(() =>
        {
            controller ??= FindAnyObjectByType<PlayerController>();
            controller?.SelectSquad(capturedType);
        });

        RectTransform selectRect = selectObject.GetComponent<RectTransform>();
        selectRect.anchorMin = new Vector2(0f, 0f);
        selectRect.anchorMax = new Vector2(0f, 1f);
        selectRect.pivot = new Vector2(0f, 0.5f);
        selectRect.anchoredPosition = Vector2.zero;
        selectRect.sizeDelta = new Vector2(188f, 0f);

        Image iconImage = CreateIcon(selectObject.transform, squadType, 4f);
        Text countText = CreateLabel(selectObject.transform, "2/3", 34f, 34f, 12, TextAnchor.MiddleCenter);
        Text modeText = CreateLabel(selectObject.transform, "AUTO", 72f, 44f, 11, TextAnchor.MiddleCenter);
        Text focusText = CreateLabel(selectObject.transform, string.Empty, 118f, 64f, 11, TextAnchor.MiddleLeft);

        float buttonX = 192f;
        const float buttonWidth = 44f;
        const float buttonGap = 4f;

        Button autoButton = CreateActionButton(rowObject.transform, "AUTO", buttonX, OnAutoClicked, squadType);
        buttonX += buttonWidth + buttonGap;
        Button holdButton = CreateActionButton(rowObject.transform, "HOLD", buttonX, OnHoldClicked, squadType);
        buttonX += buttonWidth + buttonGap;
        Button manualButton = CreateActionButton(rowObject.transform, "MAN", buttonX, OnManualClicked, squadType);
        buttonX += buttonWidth + buttonGap;
        Button moveButton = CreateActionButton(rowObject.transform, "MOVE", buttonX, OnMoveClicked, squadType);
        buttonX += buttonWidth + buttonGap;
        Button attackButton = CreateActionButton(rowObject.transform, "ATK", buttonX, OnAttackClicked, squadType);
        buttonX += buttonWidth + buttonGap;
        Button abilityButton = CreateActionButton(
            rowObject.transform,
            $"{abilityLabel} {keyLabel}",
            buttonX,
            OnAbilityClicked,
            squadType,
            width: 92f);

        Image abilityFill = CreateAbilityCooldownFill(abilityButton.transform);

        rows.Add(new SquadRow
        {
            SquadType = squadType,
            AbilityLabel = abilityLabel,
            KeyLabel = keyLabel,
            CooldownTotal = cooldownTotal,
            RowBackground = rowBackground,
            IconImage = iconImage,
            CountText = countText,
            ModeText = modeText,
            FocusText = focusText,
            AbilityCooldownFill = abilityFill,
            AutoButton = autoButton,
            HoldButton = holdButton,
            ManualButton = manualButton,
            MoveButton = moveButton,
            AttackButton = attackButton,
            AbilityButton = abilityButton
        });
    }

    static Image CreateIcon(Transform parent, UnitType squadType, float x)
    {
        GameObject iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(parent, false);

        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.color = UnitStats.For(squadType).Color;

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(x, 0f);
        iconRect.sizeDelta = new Vector2(22f, 22f);

        return iconImage;
    }

    static Text CreateLabel(
        Transform parent,
        string text,
        float x,
        float width,
        int fontSize,
        TextAnchor alignment)
    {
        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(parent, false);

        Text label = labelObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;
        label.text = text;

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(x, 0f);
        labelRect.sizeDelta = new Vector2(width, 0f);

        return label;
    }

    Button CreateActionButton(
        Transform parent,
        string label,
        float x,
        System.Action<UnitType> onClick,
        UnitType squadType,
        float width = 44f)
    {
        GameObject buttonObject = new GameObject(label + "Button");
        buttonObject.transform.SetParent(parent, false);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.18f, 0.18f, 0.85f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        UnitType capturedType = squadType;
        button.onClick.AddListener(() => onClick(capturedType));

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 0.5f);
        buttonRect.anchorMax = new Vector2(0f, 0.5f);
        buttonRect.pivot = new Vector2(0f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(x, 0f);
        buttonRect.sizeDelta = new Vector2(width, 22f);

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(buttonObject.transform, false);

        Text labelText = labelObject.AddComponent<Text>();
        labelText.font = uiFont;
        labelText.fontSize = 10;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;
        labelText.text = label;

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }

    static Image CreateAbilityCooldownFill(Transform parent)
    {
        GameObject fillObject = new GameObject("AbilityCooldownFill");
        fillObject.transform.SetParent(parent, false);
        fillObject.transform.SetAsFirstSibling();

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = new Color(0.25f, 0.55f, 0.9f, 0.75f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(1f, 1f);
        fillRect.offsetMax = new Vector2(-1f, -1f);

        return fillImage;
    }

    void OnAutoClicked(UnitType type)
    {
        controller ??= FindAnyObjectByType<PlayerController>();
        controller?.SetSquadMode(type, SquadCommandMode.Auto);
    }

    void OnHoldClicked(UnitType type)
    {
        controller ??= FindAnyObjectByType<PlayerController>();
        controller?.SetSquadMode(type, SquadCommandMode.Hold);
    }

    void OnManualClicked(UnitType type)
    {
        controller ??= FindAnyObjectByType<PlayerController>();
        controller?.SetSquadMode(type, SquadCommandMode.Manual);
    }

    void OnMoveClicked(UnitType type)
    {
        controller ??= FindAnyObjectByType<PlayerController>();
        controller?.BeginMoveTargeting(type);
    }

    void OnAttackClicked(UnitType type)
    {
        controller ??= FindAnyObjectByType<PlayerController>();
        controller?.BeginAttackTargeting(type);
    }

    void OnAbilityClicked(UnitType type)
    {
        controller ??= FindAnyObjectByType<PlayerController>();

        switch (type)
        {
            case UnitType.Defender:
                controller?.ActivateShieldForSquad(type);
                break;
            case UnitType.Attacker:
                controller?.BeginChargeTargeting(type);
                break;
            case UnitType.Archer:
                controller?.BeginVolleyTargeting(type);
                break;
        }
    }

    static string GetSquadShortName(UnitType type)
    {
        return type switch
        {
            UnitType.Defender => "DEF",
            UnitType.Attacker => "ATK",
            UnitType.Archer => "ARC",
            _ => type.ToString().ToUpper()
        };
    }

    static string GetModeLabel(SquadCommandMode mode)
    {
        return mode switch
        {
            SquadCommandMode.Hold => "HOLD",
            SquadCommandMode.Manual => "MAN",
            _ => "AUTO"
        };
    }
}
