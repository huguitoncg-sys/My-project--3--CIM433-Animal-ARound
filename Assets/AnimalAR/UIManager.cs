using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI feedbackText;
    public GameObject interactionHintText;
    public Button goHomeButton;
    public Button clearPathButton;
    public Button resetButton;
    public Button safeHomeButton;
    public Button notSafeButton;
    public Button fishButton;
    public Button iceButton;
    public Button sunButton;
    public Button cactusButton;
    public bool showAdvancedPanels = false;

    private TextMeshProUGUI progressText;
    private TextMeshProUGUI scannerText;
    private TextMeshProUGUI careText;

    private Canvas canvas;

    private void Awake()
    {
        SetupMissionUiNow();
    }

    [ContextMenu("Set Up Mission UI Now")]
    public void SetupMissionUiNow()
    {
        FindExistingReferences();
        EnsureEventSystem();
        EnsurePathButtons();
        EnsureMissionUi();
        ApplyMissionLayout();
        StyleUiForKids();
        SetProgressStep(0);
        SetPathButtonsVisible(false);
        SetDecisionButtonsVisible(false);
        SetCareButtonsVisible(false);
        ShowScanner(false);
        ShowCarePanel(false);
        ShowInteractionHint(false);
    }

    public void SetInstruction(string message)
    {
        if (instructionText != null)
        {
            instructionText.text = message;
            SetPanelVisible("InstructionText_Background", true);
        }
    }

    public void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            SetPanelVisible("FeedbackText_Background", !string.IsNullOrWhiteSpace(message));
        }
    }

    public void ShowInteractionHint(bool show)
    {
        if (interactionHintText != null)
            interactionHintText.SetActive(show);

        SetPanelVisible("InteractionHintText_Background", show);
    }

    public void SetInteractionHint(string message)
    {
        if (interactionHintText == null) return;

        TextMeshProUGUI hintText = interactionHintText.GetComponent<TextMeshProUGUI>();
        if (hintText != null)
            hintText.text = message;
    }

    public void SetPathButtonsVisible(bool visible)
    {
        if (goHomeButton != null)
            goHomeButton.gameObject.SetActive(visible);

        if (clearPathButton != null)
            clearPathButton.gameObject.SetActive(visible);
    }

    public void SetDecisionButtonsVisible(bool visible)
    {
        if (safeHomeButton != null)
            safeHomeButton.gameObject.SetActive(visible);

        if (notSafeButton != null)
            notSafeButton.gameObject.SetActive(visible);
    }

    public void SetCareButtonsVisible(bool visible)
    {
        if (fishButton != null)
            fishButton.gameObject.SetActive(visible);

        if (iceButton != null)
            iceButton.gameObject.SetActive(visible);

        if (sunButton != null)
            sunButton.gameObject.SetActive(visible);

        if (cactusButton != null)
            cactusButton.gameObject.SetActive(visible);
    }

    public void SetProgressStep(int step)
    {
        if (progressText == null) return;

        if (!showAdvancedPanels)
        {
            progressText.transform.parent.gameObject.SetActive(false);
            return;
        }

        string animal = step == 0 ? "<mark=#FFD93D88>Animal</mark>" : "Animal";
        string habitat = step == 1 ? "<mark=#FFD93D88>Habitat</mark>" : "Habitat";
        string check = step == 2 ? "<mark=#FFD93D88>Check</mark>" : "Check";
        string rescue = step == 3 ? "<mark=#FFD93D88>Rescue</mark>" : "Rescue";
        string care = step == 4 ? "<mark=#FFD93D88>Care</mark>" : "Care";

        progressText.text = $"{animal}  >  {habitat}  >  {check}  >  {rescue}  >  {care}";
    }

    public void ShowEnvironmentScanner(string homeId, bool isCorrectHome)
    {
        if (isCorrectHome)
        {
            SetFeedback("Arctic scan: cold, fish, and ice. Is this a safe home?");
        }
        else if (homeId == "desert")
        {
            SetFeedback("Desert scan: too hot, no ice, no fish. Is this safe?");
        }
        else
        {
            SetFeedback("Habitat scan: check food, shelter, and temperature.");
        }

        if (scannerText != null && showAdvancedPanels)
        {
            scannerText.text = feedbackText != null ? feedbackText.text : scannerText.text;
            ShowScanner(true);
        }
        else
        {
            ShowScanner(false);
        }
    }

    public void ShowScanner(bool visible)
    {
        if (scannerText != null)
            scannerText.transform.parent.gameObject.SetActive(visible && showAdvancedPanels);
    }

    public void ShowCareChallenge(int careScore)
    {
        UpdateCareMeter(careScore);
        ShowCarePanel(true);
        SetCareButtonsVisible(true);
    }

    public void UpdateCareMeter(int careScore)
    {
        string meter = careScore == 0 ? "[ _ _ ]" : careScore == 1 ? "[ * _ ]" : "[ * * ]";
        string message = $"Care Meter: {meter} Pick what the penguin needs.";

        SetFeedback(message);

        if (careText != null)
            careText.text = message;
    }

    public void ShowCarePanel(bool visible)
    {
        if (careText != null)
            careText.transform.parent.gameObject.SetActive(visible && showAdvancedPanels);
    }

    private void FindExistingReferences()
    {
        canvas = FindAnyObjectByType<Canvas>();

        if (instructionText == null)
            instructionText = FindText("InstructionText");

        if (feedbackText == null)
            feedbackText = FindText("FeedbackText");

        if (interactionHintText == null)
        {
            TextMeshProUGUI hintText = FindText("InteractionHintText");
            if (hintText != null)
                interactionHintText = hintText.gameObject;
        }

        if (resetButton == null)
            resetButton = FindButton("ResetButton");

        if (goHomeButton == null)
            goHomeButton = FindButton("GoHomeButton");

        if (clearPathButton == null)
            clearPathButton = FindButton("ClearPathButton");

        if (safeHomeButton == null)
            safeHomeButton = FindButton("SafeHomeButton");

        if (notSafeButton == null)
            notSafeButton = FindButton("NotSafeButton");

        if (fishButton == null)
            fishButton = FindButton("FishButton");

        if (iceButton == null)
            iceButton = FindButton("IceButton");

        if (sunButton == null)
            sunButton = FindButton("SunButton");

        if (cactusButton == null)
            cactusButton = FindButton("CactusButton");
    }

    private TextMeshProUGUI FindText(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found == null ? null : found.GetComponent<TextMeshProUGUI>();
    }

    private Button FindButton(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found == null ? null : found.GetComponent<Button>();
    }

    private void EnsurePathButtons()
    {
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (goHomeButton == null)
            goHomeButton = CreateButton("GoHomeButton", "Go Home", new Vector2(-100f, 50f));

        if (clearPathButton == null)
            clearPathButton = CreateButton("ClearPathButton", "Clear Path", new Vector2(100f, 50f));

        if (resetButton != null)
            resetButton.gameObject.SetActive(true);
    }

    private void EnsureMissionUi()
    {
        progressText = CreateOrFindTextPanel(
            "ProgressPanel",
            "ProgressText",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -118f),
            new Vector2(760f, 42f),
            new Color(0.38f, 0.12f, 0.78f, 0.92f),
            22f
        );

        scannerText = CreateOrFindTextPanel(
            "ScannerPanel",
            "ScannerText",
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(185f, 0f),
            new Vector2(330f, 310f),
            new Color(0.03f, 0.55f, 0.85f, 0.94f),
            24f
        );

        careText = CreateOrFindTextPanel(
            "CarePanel",
            "CareText",
            new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(-185f, 0f),
            new Vector2(330f, 240f),
            new Color(0.55f, 0.18f, 0.82f, 0.94f),
            24f
        );

        if (safeHomeButton == null)
            safeHomeButton = CreateButton("SafeHomeButton", "Safe Home", new Vector2(-95f, 118f));

        if (notSafeButton == null)
            notSafeButton = CreateButton("NotSafeButton", "Not Safe", new Vector2(95f, 118f));

        if (fishButton == null)
            fishButton = CreateButton("FishButton", "Fish", new Vector2(-285f, 118f));

        if (iceButton == null)
            iceButton = CreateButton("IceButton", "Ice", new Vector2(-95f, 118f));

        if (sunButton == null)
            sunButton = CreateButton("SunButton", "Sun", new Vector2(95f, 118f));

        if (cactusButton == null)
            cactusButton = CreateButton("CactusButton", "Cactus", new Vector2(285f, 118f));
    }

    private void ApplyMissionLayout()
    {
        if (canvas == null) return;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        PositionText(instructionText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(720f, 62f));
        PositionText(feedbackText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(680f, 54f));

        if (interactionHintText != null)
        {
            RectTransform hintRect = interactionHintText.GetComponent<RectTransform>();
            PositionRect(hintRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(660f, 42f));
        }

        PositionPanel(progressText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -88f), new Vector2(700f, 34f));
        PositionPanel(scannerText, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(158f, -18f), new Vector2(292f, 252f));
        PositionPanel(careText, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-158f, -18f), new Vector2(292f, 224f));

        PositionButton(resetButton, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(84f, 42f), new Vector2(132f, 46f));
        PositionButton(goHomeButton, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-82f, 48f), new Vector2(148f, 44f));
        PositionButton(clearPathButton, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(82f, 48f), new Vector2(148f, 44f));

        PositionButton(safeHomeButton, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-88f, 48f), new Vector2(158f, 44f));
        PositionButton(notSafeButton, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(88f, 48f), new Vector2(158f, 44f));

        PositionButton(fishButton, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-216f, 48f), new Vector2(104f, 44f));
        PositionButton(iceButton, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-72f, 48f), new Vector2(104f, 44f));
        PositionButton(sunButton, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(72f, 48f), new Vector2(104f, 44f));
        PositionButton(cactusButton, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(216f, 48f), new Vector2(104f, 44f));

        UpdateTextBackground(instructionText);
        UpdateTextBackground(feedbackText);

        if (interactionHintText != null)
            UpdateTextBackground(interactionHintText.GetComponent<TextMeshProUGUI>());
    }

    private void PositionText(TextMeshProUGUI text, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        if (text == null) return;

        RectTransform rect = text.GetComponent<RectTransform>();
        PositionRect(rect, anchorMin, anchorMax, anchoredPosition, size);
    }

    private void PositionPanel(TextMeshProUGUI text, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        if (text == null) return;

        RectTransform panelRect = text.transform.parent.GetComponent<RectTransform>();
        PositionRect(panelRect, anchorMin, anchorMax, anchoredPosition, size);
    }

    private void PositionButton(Button button, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        if (button == null) return;

        RectTransform rect = button.GetComponent<RectTransform>();
        PositionRect(rect, anchorMin, anchorMax, anchoredPosition, size);
    }

    private void PositionRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        if (rect == null) return;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private void UpdateTextBackground(TextMeshProUGUI text)
    {
        if (text == null) return;

        Transform existingPanel = text.transform.parent.Find(text.gameObject.name + "_Background");
        if (existingPanel == null) return;

        RectTransform textRect = text.GetComponent<RectTransform>();
        RectTransform panelRect = existingPanel.GetComponent<RectTransform>();
        if (textRect == null || panelRect == null) return;

        panelRect.anchorMin = textRect.anchorMin;
        panelRect.anchorMax = textRect.anchorMax;
        panelRect.pivot = textRect.pivot;
        panelRect.anchoredPosition = textRect.anchoredPosition;
        panelRect.sizeDelta = textRect.sizeDelta + new Vector2(30f, 18f);
    }

    private TextMeshProUGUI CreateOrFindTextPanel(
        string panelName,
        string textName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 size,
        Color panelColor,
        float fontSize
    )
    {
        Transform existingPanel = canvas.transform.Find(panelName);
        GameObject panelObject;

        if (existingPanel == null)
        {
            panelObject = new GameObject(panelName);
            panelObject.transform.SetParent(canvas.transform, false);
            panelObject.AddComponent<CanvasRenderer>();
            Image image = panelObject.AddComponent<Image>();
            image.color = panelColor;
            image.raycastTarget = false;
        }
        else
        {
            panelObject = existingPanel.gameObject;
        }

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = anchorMin;
        panelRect.anchorMax = anchorMax;
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = anchoredPosition;
        panelRect.sizeDelta = size;

        Transform existingText = panelObject.transform.Find(textName);
        GameObject textObject;

        if (existingText == null)
        {
            textObject = new GameObject(textName);
            textObject.transform.SetParent(panelObject.transform, false);
            textObject.AddComponent<CanvasRenderer>();
            textObject.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            textObject = existingText.gameObject;
        }

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.color = Color.white;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.margin = new Vector4(14f, 8f, 14f, 8f);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Shadow shadow = text.GetComponent<Shadow>();
        if (shadow == null)
            shadow = text.gameObject.AddComponent<Shadow>();

        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);

        return text;
    }

    private Button CreateButton(string objectName, string label, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(objectName);
        buttonObject.transform.SetParent(canvas.transform, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = GetButtonColor(objectName);

        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(170f, 50f);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24f;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private void StyleUiForKids()
    {
        if (canvas == null) return;

        StyleText(instructionText, 34f, Color.white, new Color(0.05f, 0.35f, 0.95f, 0.9f));
        StyleText(feedbackText, 30f, Color.white, new Color(1f, 0.48f, 0.06f, 0.92f));

        if (interactionHintText != null)
        {
            TextMeshProUGUI hintText = interactionHintText.GetComponent<TextMeshProUGUI>();
            StyleText(hintText, 28f, Color.white, new Color(0.05f, 0.7f, 0.32f, 0.92f));
        }

        StyleButton(resetButton, new Color(1f, 0.87f, 0.12f, 1f), Color.black);
        StyleButton(goHomeButton, new Color(0.15f, 0.78f, 0.35f, 1f), Color.white);
        StyleButton(clearPathButton, new Color(0.98f, 0.3f, 0.5f, 1f), Color.white);
        StyleButton(safeHomeButton, new Color(0.1f, 0.72f, 0.32f, 1f), Color.white);
        StyleButton(notSafeButton, new Color(0.95f, 0.23f, 0.28f, 1f), Color.white);
        StyleButton(fishButton, new Color(0.06f, 0.62f, 0.92f, 1f), Color.white);
        StyleButton(iceButton, new Color(0.42f, 0.86f, 1f, 1f), Color.black);
        StyleButton(sunButton, new Color(1f, 0.7f, 0.06f, 1f), Color.black);
        StyleButton(cactusButton, new Color(0.2f, 0.68f, 0.28f, 1f), Color.white);
    }

    private void StyleText(TextMeshProUGUI text, float fontSize, Color textColor, Color panelColor)
    {
        if (text == null) return;

        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;

        Shadow shadow = text.GetComponent<Shadow>();
        if (shadow == null)
            shadow = text.gameObject.AddComponent<Shadow>();

        shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(2f, -2f);

        Outline outline = text.GetComponent<Outline>();
        if (outline == null)
            outline = text.gameObject.AddComponent<Outline>();

        outline.effectColor = new Color(0f, 0f, 0f, 0.55f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        CreateOrUpdateBackground(text, panelColor);
    }

    private void CreateOrUpdateBackground(TextMeshProUGUI text, Color panelColor)
    {
        RectTransform textRect = text.GetComponent<RectTransform>();
        if (textRect == null || textRect.parent == null) return;

        string panelName = text.gameObject.name + "_Background";
        Transform existingPanel = textRect.parent.Find(panelName);

        GameObject panelObject;
        RectTransform panelRect;

        if (existingPanel == null)
        {
            panelObject = new GameObject(panelName);
            panelObject.transform.SetParent(textRect.parent, false);
            panelRect = panelObject.AddComponent<RectTransform>();
            panelObject.AddComponent<CanvasRenderer>();
            panelObject.AddComponent<Image>();
        }
        else
        {
            panelObject = existingPanel.gameObject;
            panelRect = panelObject.GetComponent<RectTransform>();
        }

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = false;

        panelRect.anchorMin = textRect.anchorMin;
        panelRect.anchorMax = textRect.anchorMax;
        panelRect.pivot = textRect.pivot;
        panelRect.anchoredPosition = textRect.anchoredPosition;
        panelRect.sizeDelta = textRect.sizeDelta + new Vector2(36f, 22f);

        panelObject.transform.SetSiblingIndex(textRect.GetSiblingIndex());
        text.transform.SetSiblingIndex(panelObject.transform.GetSiblingIndex() + 1);
    }

    private void StyleButton(Button button, Color normalColor, Color textColor)
    {
        if (button == null) return;

        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = normalColor;

        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.25f);
        colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
        button.colors = colors;

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.color = textColor;
            label.fontSize = 24f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;

            Shadow shadow = label.GetComponent<Shadow>();
            if (shadow == null)
                shadow = label.gameObject.AddComponent<Shadow>();

            shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
            shadow.effectDistance = new Vector2(1f, -1f);
        }
    }

    private Color GetButtonColor(string objectName)
    {
        if (objectName == "GoHomeButton")
            return new Color(0.15f, 0.78f, 0.35f, 1f);

        if (objectName == "ClearPathButton")
            return new Color(0.98f, 0.3f, 0.5f, 1f);

        if (objectName == "SafeHomeButton")
            return new Color(0.1f, 0.72f, 0.32f, 1f);

        if (objectName == "NotSafeButton")
            return new Color(0.95f, 0.23f, 0.28f, 1f);

        if (objectName == "FishButton")
            return new Color(0.06f, 0.62f, 0.92f, 1f);

        if (objectName == "IceButton")
            return new Color(0.42f, 0.86f, 1f, 1f);

        if (objectName == "SunButton")
            return new Color(1f, 0.7f, 0.06f, 1f);

        if (objectName == "CactusButton")
            return new Color(0.2f, 0.68f, 0.28f, 1f);

        return Color.white;
    }

    private void SetPanelVisible(string panelName, bool visible)
    {
        if (canvas == null) return;

        Transform panel = canvas.transform.Find(panelName);
        if (panel != null)
            panel.gameObject.SetActive(visible);
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
        eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
    }
}
