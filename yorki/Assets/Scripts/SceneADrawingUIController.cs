using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// SceneA 드로잉 UI 전체 상태 관리.
// 2차 범위: 좌측 도구 4종 + THICKNESS 슬라이더 + Undo/Redo/Reset 버튼.
// 팔레트/RGB 피커/Submit는 3차에서 추가.
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
    public Sprite penSelected;

    [Header("Tool Sprites — Brush")]
    public Sprite brushBase;
    public Sprite brushSelected;

    [Header("Tool Sprites — Eraser")]
    public Sprite eraserBase;
    public Sprite eraserSelected;

    [Header("Tool Sprites — Picker")]
    public Sprite pickerBase;
    public Sprite pickerSelected;

    [Header("THICKNESS")]
    public ThicknessSliderHandle thicknessSlider;
    public Text thicknessText;
    public Image previewDot;
    public int minThickness = 2;
    public int maxThickness = 24;

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

    private DrawingTool currentTool = DrawingTool.Pen;

    void Start()
    {
        WireToolButton(penButton,    DrawingTool.Pen);
        WireToolButton(brushButton,  DrawingTool.Brush);
        WireToolButton(eraserButton, DrawingTool.Eraser);
        WireToolButton(pickerButton, DrawingTool.Picker);

        WireActionButton(undoButton,  OnUndoClicked);
        WireActionButton(redoButton,  OnRedoClicked);
        WireActionButton(resetButton, OnResetClicked);

        if (thicknessSlider != null)
        {
            thicknessSlider.ValueChanged += OnThicknessChanged;
            int initialThickness = drawingCanvas != null ? drawingCanvas.GetThicknessForCurrentTool() : 8;
            float t = Mathf.InverseLerp(minThickness, maxThickness, initialThickness);
            thicknessSlider.SetNormalizedValue(t, false);
            ApplyThicknessVisual(initialThickness);
        }

        if (drawingCanvas != null)
        {
            drawingCanvas.UndoRedoChanged += RefreshActionVisuals;
            drawingCanvas.ColorPicked += OnColorPicked;
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
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(_ => SelectTool(tool));
        trigger.triggers.Add(entry);
    }

    void WireActionButton(Image image, System.Action handler)
    {
        if (image == null) return;
        EnsureRaycast(image);
        EventTrigger trigger = image.GetComponent<EventTrigger>();
        if (trigger == null) trigger = image.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(_ => handler());
        trigger.triggers.Add(entry);
    }

    void EnsureRaycast(Image image)
    {
        image.raycastTarget = true;
    }

    public void SelectTool(DrawingTool tool)
    {
        currentTool = tool;
        if (drawingCanvas != null) drawingCanvas.SetTool(tool);

        SetToolSprite(penButton,    tool == DrawingTool.Pen    ? penSelected    : penBase);
        SetToolSprite(brushButton,  tool == DrawingTool.Brush  ? brushSelected  : brushBase);
        SetToolSprite(eraserButton, tool == DrawingTool.Eraser ? eraserSelected : eraserBase);
        SetToolSprite(pickerButton, tool == DrawingTool.Picker ? pickerSelected : pickerBase);

        if (drawingCanvas != null)
        {
            int newThickness = drawingCanvas.GetThicknessForCurrentTool();
            float t = Mathf.InverseLerp(minThickness, maxThickness, newThickness);
            if (thicknessSlider != null) thicknessSlider.SetNormalizedValue(t, false);
            ApplyThicknessVisual(newThickness);
        }
    }

    void SetToolSprite(Image image, Sprite sprite)
    {
        if (image == null || sprite == null) return;
        image.sprite = sprite;
    }

    void OnThicknessChanged(float t)
    {
        int thickness = Mathf.RoundToInt(Mathf.Lerp(minThickness, maxThickness, t));
        if (drawingCanvas != null) drawingCanvas.SetBrushSize(thickness);
        ApplyThicknessVisual(thickness);
    }

    void ApplyThicknessVisual(int thickness)
    {
        if (thicknessText != null) thicknessText.text = thickness + " px";
        if (previewDot != null)
        {
            float diameter = Mathf.Lerp(6f, 36f, Mathf.InverseLerp(minThickness, maxThickness, thickness));
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

    void OnColorPicked(Color color)
    {
        if (color.a <= 0f) return;
        color.a = 1f;
        if (drawingCanvas != null) drawingCanvas.SetBrushColor(color);
        SelectTool(DrawingTool.Pen);
    }
}
