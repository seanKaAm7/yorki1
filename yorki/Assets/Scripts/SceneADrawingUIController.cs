using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// SceneA 드로잉 UI 전체 상태 관리.
// 좌측 도구 4종 (base/selecting/selected 3상태) + THICKNESS + Undo/Redo/Reset + 팔레트 8칸.
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

    [Header("Action Sprites")]
    public Sprite undoActive;
    public Sprite undoInactive;
    public Sprite redoActive;
    public Sprite redoInactive;
    public Sprite resetSprite;

    [Header("Palette (8 slots)")]
    public Image[] paletteSlots;
    public Color[] paletteDefaultColors;

    private DrawingTool currentTool = DrawingTool.Pen;
    private int selectedPaletteIndex = 0;

    void Start()
    {
        WireToolButton(penButton,    DrawingTool.Pen);
        WireToolButton(brushButton,  DrawingTool.Brush);
        WireToolButton(eraserButton, DrawingTool.Eraser);
        WireToolButton(pickerButton, DrawingTool.Picker);

        WireActionButton(undoButton,  OnUndoClicked);
        WireActionButton(redoButton,  OnRedoClicked);
        WireActionButton(resetButton, OnResetClicked);

        WirePaletteSlots();

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
            if (drawingCanvas != null) drawingCanvas.SetBrushColor(paletteSlots[0].color);
        }

        SelectTool(DrawingTool.Pen);
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
            EventTrigger trigger = slot.GetComponent<EventTrigger>();
            if (trigger == null) trigger = slot.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();
            EventTrigger.Entry click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener(_ => SelectPaletteSlot(idx));
            trigger.triggers.Add(click);
        }
    }

    void EnsureRaycast(Image image)
    {
        image.raycastTarget = true;
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
        if (drawingCanvas != null) drawingCanvas.SetBrushColor(c);

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

        if (drawingCanvas != null) drawingCanvas.SetBrushColor(color);
        SelectTool(DrawingTool.Pen);
    }
}
