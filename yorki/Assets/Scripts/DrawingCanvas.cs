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

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();
        drawTexture = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        drawTexture.filterMode = FilterMode.Point;
        ResetCanvas();
        rawImage.texture = drawTexture;
    }

    void ResetCanvas()
    {
        Color[] pixels = new Color[canvasWidth * canvasHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = backgroundColor;
        drawTexture.SetPixels(pixels);
        drawTexture.Apply();
    }

    public void ClearCanvas()
    {
        ResetCanvas();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
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
        PaintStroke(lastDrawPos, coord); // PaintStroke 내부에서 Apply() 호출
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
        float x = (localPos.x / rectTransform.rect.width + 0.5f) * canvasWidth;
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
