using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// SceneA 드로잉 UI 전체 상태 관리.
// 좌측 도구 4종 + THICKNESS + Undo/Redo/Reset/Submit + 팔레트 8칸 + COLOR RGB 박스.
public class SceneADrawingUIController : MonoBehaviour
{
    [Header("Drawing Canvas")]
    public DrawingCanvas drawingCanvas;

    [Header("Left Tool Buttons (Image)")]
    public Image penButton;
    public Image brushButton;
    public Image eraserButton;
    public Image pickerButton;

    [Header("Tool Sprites — Pen")]
    public Sprite penBase;
    public Sprite penSelecting;
    public Sprite penSelected;

    [Header("Tool Sprites — Brush")]
    public Sprite brushBase;
    public Sprite brushSelecting;
    public Sprite brushSelected;

    [Header("Tool Sprites — Eraser")]
    public Sprite eraserBase;
    public Sprite eraserSelecting;
    public Sprite eraserSelected;

    [Header("Tool Sprites — Picker")]
    public Sprite pickerBase;
    public Sprite pickerSelecting;
    public Sprite pickerSelected;

    [Header("THICKNESS")]
    public ThicknessSliderHandle thicknessSlider;
    public Text thicknessText;
    public Image previewDot;
    // 도구별 굵기 범위는 DrawingCanvas가 보유. 슬라이더 t는 도구의 [min,max]에 매핑.

    [Header("Right Action Buttons (Image)")]
    public Image undoButton;
    public Image redoButton;
    public Image resetButton;
    public Image submitButton;

    [Header("Action Sprites")]
    public Sprite undoActive;
    public Sprite undoInactive;
    public Sprite redoActive;
    public Sprite redoInactive;
    public Sprite resetSprite;

    [Header("Palette (8 slots)")]
    public Image[] paletteSlots;

    [Header("COLOR / RGB Box")]
    public RawImage saturationValueArea;
    public RawImage hueBar;
    public InputField rInput;
    public InputField gInput;
    public InputField bInput;
    public Image colorPreview;

    private DrawingTool currentTool = DrawingTool.Pen;
    private int selectedPaletteIndex = 0;
    private float currentHue = 0.08f;
    private float currentSaturation = 0.28f;
    private float currentValue = 0.91f;
    private bool isUpdatingRgbFields;
    private Texture2D saturationValueTexture;
    private Texture2D hueTexture;
    const int colorTextureSize = 128;

    void Start()
    {
        WireToolButton(penButton,    DrawingTool.Pen);
        WireToolButton(brushButton,  DrawingTool.Brush);
        WireToolButton(eraserButton, DrawingTool.Eraser);
        WireToolButton(pickerButton, DrawingTool.Picker);

        WireActionButton(undoButton,  OnUndoClicked);
        WireActionButton(redoButton,  OnRedoClicked);
        WireActionButton(resetButton, OnResetClicked);
        WireActionButton(submitButton, OnSubmitClicked);

        WirePaletteSlots();
        WireColorBox();

        if (thicknessSlider != null)
        {
            thicknessSlider.ValueChanged += OnThicknessChanged;
            // 시작 시 슬라이더 중앙(t=0.5) → 현재 도구 default 굵기
            thicknessSlider.SetNormalizedValue(0.5f, false);
            int initialThickness = drawingCanvas != null ? drawingCanvas.GetDefaultThicknessForTool(currentTool) : 6;
            ApplyThicknessVisual(initialThickness);
        }

        if (drawingCanvas != null)
        {
            drawingCanvas.UndoRedoChanged += RefreshActionVisuals;
            drawingCanvas.ColorPicked += OnColorPicked;
        }

        // 시작 시 첫 슬롯 색을 활성 색상으로 적용
        if (paletteSlots != null && paletteSlots.Length > 0 && paletteSlots[0] != null)
        {
            selectedPaletteIndex = 0;
            ApplyColorToSelection(paletteSlots[0].color, true);
        }

        SelectTool(DrawingTool.Pen);
        RefreshPaletteSelectionVisuals();
        RefreshActionVisuals();
    }

    void OnDestroy()
    {
        if (drawingCanvas != null)
        {
            drawingCanvas.UndoRedoChanged -= RefreshActionVisuals;
            drawingCanvas.ColorPicked -= OnColorPicked;
        }
        if (thicknessSlider != null)
            thicknessSlider.ValueChanged -= OnThicknessChanged;

        if (saturationValueTexture != null) Destroy(saturationValueTexture);
        if (hueTexture != null) Destroy(hueTexture);
    }

    void WireToolButton(Image image, DrawingTool tool)
    {
        if (image == null) return;
        EnsureRaycast(image);
        EventTrigger trigger = image.GetComponent<EventTrigger>();
        if (trigger == null) trigger = image.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        EventTrigger.Entry down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => SetToolSpriteByState(image, tool, ToolVisualState.Selecting));
        trigger.triggers.Add(down);

        EventTrigger.Entry up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ => RefreshToolVisuals());
        trigger.triggers.Add(up);

        EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => RefreshToolVisuals());
        trigger.triggers.Add(exit);

        EventTrigger.Entry click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        click.callback.AddListener(_ => SelectTool(tool));
        trigger.triggers.Add(click);
    }

    void WireActionButton(Image image, System.Action handler)
    {
        if (image == null) return;
        EnsureRaycast(image);
        EventTrigger trigger = image.GetComponent<EventTrigger>();
        if (trigger == null) trigger = image.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(_ => handler());
        trigger.triggers.Add(entry);
    }

    void WirePaletteSlots()
    {
        if (paletteSlots == null) return;
        for (int i = 0; i < paletteSlots.Length; i++)
        {
            Image slot = paletteSlots[i];
            if (slot == null) continue;
            int idx = i;
            EnsureRaycast(slot);
            EnsurePaletteOutline(slot, i == selectedPaletteIndex);
            EventTrigger trigger = slot.GetComponent<EventTrigger>();
            if (trigger == null) trigger = slot.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();
            EventTrigger.Entry click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener(_ => SelectPaletteSlot(idx));
            trigger.triggers.Add(click);
        }
    }

    void EnsureRaycast(Graphic graphic)
    {
        if (graphic != null) graphic.raycastTarget = true;
    }

    void WireColorBox()
    {
        WireColorDragArea(saturationValueArea, OnSaturationValuePointer);
        WireColorDragArea(hueBar, OnHuePointer);

        WireRgbInput(rInput);
        WireRgbInput(gInput);
        WireRgbInput(bInput);

        GenerateHueTexture();
        RefreshColorBoxVisuals();
    }

    void WireColorDragArea(Graphic graphic, System.Action<PointerEventData> handler)
    {
        if (graphic == null) return;
        EnsureRaycast(graphic);
        EventTrigger trigger = graphic.GetComponent<EventTrigger>();
        if (trigger == null) trigger = graphic.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        EventTrigger.Entry down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(data => handler((PointerEventData)data));
        trigger.triggers.Add(down);

        EventTrigger.Entry drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        drag.callback.AddListener(data => handler((PointerEventData)data));
        trigger.triggers.Add(drag);
    }

    void WireRgbInput(InputField input)
    {
        if (input == null) return;
        input.contentType = InputField.ContentType.IntegerNumber;
        input.characterLimit = 3;
        input.onEndEdit.RemoveListener(OnRgbInputEdited);
        input.onEndEdit.AddListener(OnRgbInputEdited);
    }

    enum ToolVisualState { Base, Selecting, Selected }

    public void SelectTool(DrawingTool tool)
    {
        currentTool = tool;
        if (drawingCanvas != null) drawingCanvas.SetTool(tool);

        RefreshToolVisuals();

        // 도구를 바꾸면 슬라이더는 항상 중앙으로 리셋 + 그 도구의 default 굵기 적용
        if (drawingCanvas != null && tool != DrawingTool.Picker)
        {
            if (thicknessSlider != null) thicknessSlider.SetNormalizedValue(0.5f, false);
            int defaultThickness = drawingCanvas.GetDefaultThicknessForTool(tool);
            ApplyThicknessVisual(defaultThickness);
        }
    }

    void RefreshToolVisuals()
    {
        SetToolSpriteByState(penButton,    DrawingTool.Pen,    currentTool == DrawingTool.Pen    ? ToolVisualState.Selected : ToolVisualState.Base);
        SetToolSpriteByState(brushButton,  DrawingTool.Brush,  currentTool == DrawingTool.Brush  ? ToolVisualState.Selected : ToolVisualState.Base);
        SetToolSpriteByState(eraserButton, DrawingTool.Eraser, currentTool == DrawingTool.Eraser ? ToolVisualState.Selected : ToolVisualState.Base);
        SetToolSpriteByState(pickerButton, DrawingTool.Picker, currentTool == DrawingTool.Picker ? ToolVisualState.Selected : ToolVisualState.Base);
    }

    void SetToolSpriteByState(Image image, DrawingTool tool, ToolVisualState state)
    {
        if (image == null) return;
        Sprite s = GetSpriteForState(tool, state);
        if (s != null) image.sprite = s;
    }

    Sprite GetSpriteForState(DrawingTool tool, ToolVisualState state)
    {
        switch (tool)
        {
            case DrawingTool.Pen:    return state == ToolVisualState.Selected ? penSelected    : (state == ToolVisualState.Selecting ? penSelecting    : penBase);
            case DrawingTool.Brush:  return state == ToolVisualState.Selected ? brushSelected  : (state == ToolVisualState.Selecting ? brushSelecting  : brushBase);
            case DrawingTool.Eraser: return state == ToolVisualState.Selected ? eraserSelected : (state == ToolVisualState.Selecting ? eraserSelecting : eraserBase);
            case DrawingTool.Picker: return state == ToolVisualState.Selected ? pickerSelected : (state == ToolVisualState.Selecting ? pickerSelecting : pickerBase);
        }
        return null;
    }

    void OnThicknessChanged(float t)
    {
        if (drawingCanvas == null) return;
        int mn, mx;
        drawingCanvas.GetThicknessRangeForTool(currentTool, out mn, out mx);
        int thickness = Mathf.RoundToInt(Mathf.Lerp(mn, mx, t));
        drawingCanvas.SetBrushSize(thickness);
        ApplyThicknessVisual(thickness);
    }

    void ApplyThicknessVisual(int thickness)
    {
        if (thicknessText != null) thicknessText.text = thickness + " px";
        if (previewDot != null && drawingCanvas != null)
        {
            int mn, mx;
            drawingCanvas.GetThicknessRangeForTool(currentTool, out mn, out mx);
            float diameter = Mathf.Lerp(6f, 36f, Mathf.InverseLerp(mn, mx, thickness));
            previewDot.rectTransform.sizeDelta = new Vector2(diameter, diameter);
        }
    }

    void OnUndoClicked()
    {
        if (drawingCanvas != null && drawingCanvas.CanUndo) drawingCanvas.Undo();
    }

    void OnRedoClicked()
    {
        if (drawingCanvas != null && drawingCanvas.CanRedo) drawingCanvas.Redo();
    }

    void OnResetClicked()
    {
        if (drawingCanvas != null) drawingCanvas.ClearCanvas();
    }

    void OnSubmitClicked()
    {
        if (drawingCanvas == null)
        {
            Debug.LogWarning("[SceneADrawingUIController] Submit 실패 — drawingCanvas가 연결되지 않음");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.drawingCanvas = drawingCanvas;
            GameManager.Instance.OnSubmit();
            return;
        }

        CustomerData fallbackCustomer = Resources.Load<CustomerData>("Customers/SampleCustomer");
        if (fallbackCustomer == null)
        {
            Debug.LogWarning("[SceneADrawingUIController] Submit fallback — SampleCustomer가 없어 ResultGood으로 복귀");
            SceneTransition.EnsureInstance().SceneAToTalkScene(TalkScenePhase.ResultGood);
            return;
        }

        Texture2D playerTex = drawingCanvas.GetFlattenedTextureForScoring();
        int score = ScoreCalculator.Calculate(playerTex, fallbackCustomer);
        ReactionResult result = ReactionSystem.Evaluate(score, fallbackCustomer);
        TalkScenePhase nextPhase = ReactionSystem.IsGoodResult(result.level) ? TalkScenePhase.ResultGood : TalkScenePhase.ResultBad;
        Debug.Log($"[SceneADrawingUIController] Submit fallback 점수: {score} | 반응: {result.level}");
        SceneTransition.EnsureInstance().SceneAToTalkScene(nextPhase);
    }

    void RefreshActionVisuals()
    {
        if (drawingCanvas == null) return;
        if (undoButton != null && undoActive != null && undoInactive != null)
            undoButton.sprite = drawingCanvas.CanUndo ? undoActive : undoInactive;
        if (redoButton != null && redoActive != null && redoInactive != null)
            redoButton.sprite = drawingCanvas.CanRedo ? redoActive : redoInactive;
    }

    void SelectPaletteSlot(int idx)
    {
        if (paletteSlots == null || idx < 0 || idx >= paletteSlots.Length) return;
        if (paletteSlots[idx] == null) return;

        selectedPaletteIndex = idx;
        Color c = paletteSlots[idx].color;
        ApplyColorToSelection(c, true);
        RefreshPaletteSelectionVisuals();

        // 색 선택 시 그리기 도구가 아니면 Pen으로 자동 전환
        if (currentTool == DrawingTool.Picker || currentTool == DrawingTool.Eraser)
            SelectTool(DrawingTool.Pen);
    }

    void OnColorPicked(Color color)
    {
        if (color.a <= 0f) return;
        color.a = 1f;

        // 스포이드로 찍은 색은 현재 선택된 팔레트 슬롯에 반영
        if (paletteSlots != null && selectedPaletteIndex >= 0 && selectedPaletteIndex < paletteSlots.Length
            && paletteSlots[selectedPaletteIndex] != null)
        {
            paletteSlots[selectedPaletteIndex].color = color;
        }

        ApplyColorToSelection(color, true);
        RefreshPaletteSelectionVisuals();
        SelectTool(DrawingTool.Pen);
    }

    void OnSaturationValuePointer(PointerEventData eventData)
    {
        if (saturationValueArea == null) return;
        float x, y;
        if (!TryGetNormalizedPoint(saturationValueArea.rectTransform, eventData, out x, out y)) return;

        currentSaturation = x;
        currentValue = y;
        ApplyColorToSelection(Color.HSVToRGB(currentHue, currentSaturation, currentValue), false);
    }

    void OnHuePointer(PointerEventData eventData)
    {
        if (hueBar == null) return;
        float x, y;
        if (!TryGetNormalizedPoint(hueBar.rectTransform, eventData, out x, out y)) return;

        currentHue = 1f - y;
        ApplyColorToSelection(Color.HSVToRGB(currentHue, currentSaturation, currentValue), false);
    }

    bool TryGetNormalizedPoint(RectTransform rt, PointerEventData eventData, out float x, out float y)
    {
        x = 0f;
        y = 0f;
        if (rt == null) return false;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, eventData.position, eventData.pressEventCamera, out localPos);

        x = Mathf.Clamp01(localPos.x / rt.rect.width + 0.5f);
        y = Mathf.Clamp01(localPos.y / rt.rect.height + 0.5f);
        return true;
    }

    void OnRgbInputEdited(string _)
    {
        if (isUpdatingRgbFields) return;

        int r = ParseRgbField(rInput, 0);
        int g = ParseRgbField(gInput, 0);
        int b = ParseRgbField(bInput, 0);
        Color color = new Color32((byte)r, (byte)g, (byte)b, 255);

        Color.RGBToHSV(color, out currentHue, out currentSaturation, out currentValue);
        ApplyColorToSelection(color, false);
    }

    int ParseRgbField(InputField input, int fallback)
    {
        if (input == null) return fallback;
        int value;
        if (!int.TryParse(input.text, out value)) value = fallback;
        return Mathf.Clamp(value, 0, 255);
    }

    void ApplyColorToSelection(Color color, bool syncHsv)
    {
        color.a = 1f;
        if (syncHsv)
            Color.RGBToHSV(color, out currentHue, out currentSaturation, out currentValue);

        if (paletteSlots != null && selectedPaletteIndex >= 0 && selectedPaletteIndex < paletteSlots.Length
            && paletteSlots[selectedPaletteIndex] != null)
        {
            paletteSlots[selectedPaletteIndex].color = color;
        }

        if (drawingCanvas != null)
            drawingCanvas.SetBrushColor(color);

        RefreshColorBoxVisuals();
    }

    void RefreshColorBoxVisuals()
    {
        GenerateSaturationValueTexture();
        if (colorPreview != null)
            colorPreview.color = Color.HSVToRGB(currentHue, currentSaturation, currentValue);

        Color32 rgb = Color.HSVToRGB(currentHue, currentSaturation, currentValue);
        isUpdatingRgbFields = true;
        SetRgbField(rInput, rgb.r);
        SetRgbField(gInput, rgb.g);
        SetRgbField(bInput, rgb.b);
        isUpdatingRgbFields = false;
    }

    void SetRgbField(InputField input, byte value)
    {
        if (input == null) return;
        input.SetTextWithoutNotify(value.ToString());
    }

    void GenerateHueTexture()
    {
        if (hueBar == null) return;
        if (hueTexture == null)
        {
            hueTexture = new Texture2D(16, colorTextureSize, TextureFormat.RGBA32, false);
            hueTexture.filterMode = FilterMode.Point;
            hueTexture.wrapMode = TextureWrapMode.Clamp;
        }

        for (int y = 0; y < colorTextureSize; y++)
        {
            float hue = 1f - (float)y / (colorTextureSize - 1);
            Color c = Color.HSVToRGB(hue, 1f, 1f);
            for (int x = 0; x < 16; x++)
                hueTexture.SetPixel(x, y, c);
        }
        hueTexture.Apply();
        hueBar.texture = hueTexture;
    }

    void GenerateSaturationValueTexture()
    {
        if (saturationValueArea == null) return;
        if (saturationValueTexture == null)
        {
            saturationValueTexture = new Texture2D(colorTextureSize, colorTextureSize, TextureFormat.RGBA32, false);
            saturationValueTexture.filterMode = FilterMode.Point;
            saturationValueTexture.wrapMode = TextureWrapMode.Clamp;
        }

        for (int y = 0; y < colorTextureSize; y++)
        {
            float value = (float)y / (colorTextureSize - 1);
            for (int x = 0; x < colorTextureSize; x++)
            {
                float saturation = (float)x / (colorTextureSize - 1);
                saturationValueTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, saturation, value));
            }
        }
        saturationValueTexture.Apply();
        saturationValueArea.texture = saturationValueTexture;
    }

    void RefreshPaletteSelectionVisuals()
    {
        if (paletteSlots == null) return;
        for (int i = 0; i < paletteSlots.Length; i++)
        {
            if (paletteSlots[i] == null) continue;
            EnsurePaletteOutline(paletteSlots[i], i == selectedPaletteIndex);
        }
    }

    void EnsurePaletteOutline(Image slot, bool selected)
    {
        Outline outline = slot.GetComponent<Outline>();
        if (outline == null) outline = slot.gameObject.AddComponent<Outline>();
        outline.enabled = selected;
        outline.effectColor = new Color(1f, 0.92f, 0.42f, 1f);
        outline.effectDistance = new Vector2(3f, -3f);
    }

}
