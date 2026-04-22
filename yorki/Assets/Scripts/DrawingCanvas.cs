using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public int canvasWidth = 512;
    public int canvasHeight = 512;
    public Color backgroundColor = Color.white;
    public Color brushColor = Color.black;
    public int brushSize = 6;

    private Texture2D drawTexture;
    private RawImage rawImage;
    private RectTransform rectTransform;
    private Vector2 lastDrawPos;
    private bool isDrawing = false;

    // Undo
    private readonly List<Color32[]> undoStack = new List<Color32[]>();
    private const int MAX_UNDO = 20;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();
        drawTexture = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        drawTexture.filterMode = FilterMode.Point;
        ResetCanvas();
        rawImage.texture = drawTexture;
    }

    void Update()
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool cmd  = Input.GetKey(KeyCode.LeftCommand)  || Input.GetKey(KeyCode.RightCommand);
        if ((ctrl || cmd) && Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
    }

    void ResetCanvas()
    {
        Color32[] pixels = new Color32[canvasWidth * canvasHeight];
        Color32 bg = backgroundColor;
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
    }

    void Undo()
    {
        if (undoStack.Count == 0) return;
        Color32[] prev = undoStack[undoStack.Count - 1];
        undoStack.RemoveAt(undoStack.Count - 1);
        drawTexture.SetPixels32(prev);
        drawTexture.Apply();
    }

    public void ClearCanvas()
    {
        SaveSnapshot();
        ResetCanvas();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SaveSnapshot(); // 획 시작 전 현재 상태 저장
        isDrawing = true;
        Vector2 coord = ScreenToTexture(eventData);
        lastDrawPos = coord;
        PaintDot((int)coord.x, (int)coord.y, brushSize, brushColor);
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
        float dist = Vector2.Distance(from, to);
        int steps = Mathf.Max(1, Mathf.RoundToInt(dist * 2));
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t));
            PaintDot(x, y, brushSize, brushColor);
        }
        drawTexture.Apply();
    }

    public void SetBrushColor(Color color) { brushColor = color; }
    public void SetEraser() { brushColor = backgroundColor; }
    public void SetBrushSize(int size) { brushSize = size; }
    public Texture2D GetDrawTexture() { return drawTexture; }
}
