using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum DrawingTool { Pen, Brush, Eraser, Picker }

public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public int canvasWidth = 512;
    public int canvasHeight = 512;
    public Color backgroundColor = Color.white;
    public bool transparentBackground = false;
    public Color brushColor = Color.black;
    public int brushSize = 6;

    [Header("Tool")]
    public DrawingTool currentTool = DrawingTool.Pen;
    public int penThickness = 6;
    public int brushThickness = 12;
    public int eraserThickness = 18;
    [Range(0f, 1f)] public float brushAlpha = 0.7f;

    private Texture2D drawTexture;
    private RawImage rawImage;
    private RectTransform rectTransform;
    private Vector2 lastDrawPos;
    private bool isDrawing = false;
    private Color savedDrawColor = Color.black;

    private readonly List<Color32[]> undoStack = new List<Color32[]>();
    private readonly List<Color32[]> redoStack = new List<Color32[]>();
    private const int MAX_UNDO = 20;

    public System.Action UndoRedoChanged;
    public System.Action<Color> ColorPicked;

    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();
        drawTexture = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        drawTexture.filterMode = FilterMode.Point;
        ResetCanvas();
        rawImage.texture = drawTexture;
        savedDrawColor = brushColor;
    }

    void Update()
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool cmd  = Input.GetKey(KeyCode.LeftCommand)  || Input.GetKey(KeyCode.RightCommand);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if ((ctrl || cmd) && Input.GetKeyDown(KeyCode.Z))
        {
            if (shift) Redo();
            else Undo();
        }
    }

    void ResetCanvas()
    {
        Color32[] pixels = new Color32[canvasWidth * canvasHeight];
        Color32 bg = transparentBackground ? new Color32(0, 0, 0, 0) : backgroundColor;
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = bg;
        drawTexture.SetPixels32(pixels);
        drawTexture.Apply();
    }

    void SaveSnapshot()
    {
        Color32[] snapshot = (Color32[])drawTexture.GetPixels32().Clone();
        undoStack.Add(snapshot);
        if (undoStack.Count > MAX_UNDO)
            undoStack.RemoveAt(0);
        redoStack.Clear();
        UndoRedoChanged?.Invoke();
    }

    public void Undo()
    {
        if (undoStack.Count == 0) return;
        Color32[] currentState = (Color32[])drawTexture.GetPixels32().Clone();
        redoStack.Add(currentState);
        if (redoStack.Count > MAX_UNDO) redoStack.RemoveAt(0);

        Color32[] prev = undoStack[undoStack.Count - 1];
        undoStack.RemoveAt(undoStack.Count - 1);
        drawTexture.SetPixels32(prev);
        drawTexture.Apply();
        UndoRedoChanged?.Invoke();
    }

    public void Redo()
    {
        if (redoStack.Count == 0) return;
        Color32[] currentState = (Color32[])drawTexture.GetPixels32().Clone();
        undoStack.Add(currentState);
        if (undoStack.Count > MAX_UNDO) undoStack.RemoveAt(0);

        Color32[] next = redoStack[redoStack.Count - 1];
        redoStack.RemoveAt(redoStack.Count - 1);
        drawTexture.SetPixels32(next);
        drawTexture.Apply();
        UndoRedoChanged?.Invoke();
    }

    public void ClearCanvas()
    {
        SaveSnapshot();
        ResetCanvas();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentTool == DrawingTool.Picker)
        {
            Color picked = GetColorAtScreenPosition(eventData.position, eventData.pressEventCamera);
            ColorPicked?.Invoke(picked);
            return;
        }

        SaveSnapshot();
        isDrawing = true;
        Vector2 coord = ScreenToTexture(eventData);
        lastDrawPos = coord;
        PaintDot((int)coord.x, (int)coord.y, brushSize, GetActiveColor());
        drawTexture.Apply();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDrawing) return;
        Vector2 coord = ScreenToTexture(eventData);
        PaintStroke(lastDrawPos, coord);
        lastDrawPos = coord;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
    }

    Vector2 ScreenToTexture(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out localPos);
        float x = (localPos.x / rectTransform.rect.width  + 0.5f) * canvasWidth;
        float y = (localPos.y / rectTransform.rect.height + 0.5f) * canvasHeight;
        return new Vector2(x, y);
    }

    Color GetActiveColor()
    {
        if (currentTool == DrawingTool.Eraser)
            return transparentBackground ? new Color(0f, 0f, 0f, 0f) : backgroundColor;

        Color c = brushColor;
        if (currentTool == DrawingTool.Brush)
            c.a *= brushAlpha;
        return c;
    }

    void PaintDot(int cx, int cy, int radius, Color color)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                        drawTexture.SetPixel(px, py, color);
                }
            }
        }
    }

    void PaintStroke(Vector2 from, Vector2 to)
    {
        Color color = GetActiveColor();
        float dist = Vector2.Distance(from, to);
        int steps = Mathf.Max(1, Mathf.RoundToInt(dist * 2));
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t));
            PaintDot(x, y, brushSize, color);
        }
        drawTexture.Apply();
    }

    public void SetTool(DrawingTool tool)
    {
        currentTool = tool;
        switch (tool)
        {
            case DrawingTool.Pen:
                brushColor = savedDrawColor;
                brushSize = penThickness;
                break;
            case DrawingTool.Brush:
                brushColor = savedDrawColor;
                brushSize = brushThickness;
                break;
            case DrawingTool.Eraser:
                brushSize = eraserThickness;
                break;
            case DrawingTool.Picker:
                break;
        }
    }

    public void SetBrushColor(Color color)
    {
        savedDrawColor = color;
        if (currentTool != DrawingTool.Eraser)
            brushColor = color;
    }

    public void SetEraser()
    {
        SetTool(DrawingTool.Eraser);
    }

    public void SetBrushSize(int size)
    {
        brushSize = Mathf.Clamp(size, 1, 64);
        switch (currentTool)
        {
            case DrawingTool.Pen:    penThickness    = brushSize; break;
            case DrawingTool.Brush:  brushThickness  = brushSize; break;
            case DrawingTool.Eraser: eraserThickness = brushSize; break;
        }
    }

    public int GetThicknessForCurrentTool()
    {
        switch (currentTool)
        {
            case DrawingTool.Pen:    return penThickness;
            case DrawingTool.Brush:  return brushThickness;
            case DrawingTool.Eraser: return eraserThickness;
            default:                 return brushSize;
        }
    }

    public Color GetColorAtScreenPosition(Vector2 screenPos, Camera cam)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, cam, out localPos);
        int x = Mathf.RoundToInt((localPos.x / rectTransform.rect.width  + 0.5f) * canvasWidth);
        int y = Mathf.RoundToInt((localPos.y / rectTransform.rect.height + 0.5f) * canvasHeight);
        if (x < 0 || x >= canvasWidth || y < 0 || y >= canvasHeight) return Color.clear;
        return drawTexture.GetPixel(x, y);
    }

    public Texture2D GetDrawTexture() { return drawTexture; }

    public Texture2D GetFlattenedTextureForScoring()
    {
        Texture2D flattened = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        Color32[] source = drawTexture.GetPixels32();
        Color32[] pixels = new Color32[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            Color src = source[i];
            Color mixed = Color.Lerp(backgroundColor, src, src.a);
            mixed.a = 1f;
            pixels[i] = mixed;
        }

        flattened.SetPixels32(pixels);
        flattened.Apply();
        return flattened;
    }
}
