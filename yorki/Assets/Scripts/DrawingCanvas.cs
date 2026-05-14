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
    // 도구별 굵기 — default가 슬라이더 중앙(t=0.5)에서 나오도록 min/max 좌우 대칭으로 잡음.
    public int penMin = 1,    penDefault = 6,  penMax = 11;
    public int brushMin = 2,  brushDefault = 12, brushMax = 22;
    public int eraserMin = 3, eraserDefault = 18, eraserMax = 33;
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

    void Start() // 초기화. 텍스처 생성하고 캔버스 초기화한 뒤 RawImage에 적용. 드로잉 색상 저장.
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();
        drawTexture = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        drawTexture.filterMode = FilterMode.Point;
        ResetCanvas();
        rawImage.texture = drawTexture;
        savedDrawColor = brushColor;
    }

    void Update() // 단축키 처리. Ctrl+Z 또는 Cmd+Z로 되돌리기, Shift+Ctrl+Z 또는 Shift+Cmd+Z로 다시 실행. UI 버튼과 동일한 기능이지만 키보드로도 접근 가능하도록 함.
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

    void ResetCanvas() // 캔버스 초기화. 배경색 또는 투명색으로 전체 픽셀 채우고 텍스처 적용.
    {
        Color32[] pixels = new Color32[canvasWidth * canvasHeight];
        Color32 bg = transparentBackground ? new Color32(0, 0, 0, 0) : backgroundColor;
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = bg;
        drawTexture.SetPixels32(pixels);
        drawTexture.Apply();
    }

    void SaveSnapshot() // 현재 상태를 undo 스택에 저장. redo 스택은 비움.
    {
        Color32[] snapshot = (Color32[])drawTexture.GetPixels32().Clone();
        undoStack.Add(snapshot);
        if (undoStack.Count > MAX_UNDO)
            undoStack.RemoveAt(0);
        redoStack.Clear();
        UndoRedoChanged?.Invoke();
    }

    public void Undo() // 되돌리기. UI 버튼에서 호출. 현재 상태를 redo 스택에 저장한 뒤 undo 스택에서 이전 상태를 꺼내서 적용. undo/redo 스택 크기 제한해서 메모리 과다 사용 방지.
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

    public void Redo() //  되돌리기 취소. Undo에서 이전 상태로 돌아갔다가 다시 앞으로 갈 때 호출됨.
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

    public void ClearCanvas() // 캔버스 초기화. UI 버튼에서 호출. 초기화 전에 현재 상태 저장해서 되돌리기 가능하도록 함.
    {
        SaveSnapshot();
        ResetCanvas();
    }

    public void OnPointerDown(PointerEventData eventData) // 드로잉 시작. 마우스/터치 누를 때 호출됨. 픽커 도구면 색상 선택, 아니면 드로잉 시작.
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

    public void OnDrag(PointerEventData eventData) // 드로잉 중. 마우스/터치 이동할 때마다 호출됨. 이전 위치에서 현재 위치까지 선분을 따라 점 찍어서 그림.
    {
        if (!isDrawing) return;
        Vector2 coord = ScreenToTexture(eventData);
        PaintStroke(lastDrawPos, coord);
        lastDrawPos = coord;
    }

    public void OnPointerUp(PointerEventData eventData) // 드로잉 종료. OnDrag에서 그리다가 마우스/터치 뗄 때 호출됨.
    {
        isDrawing = false;
    }

    Vector2 ScreenToTexture(PointerEventData eventData) // 스크린 좌표를 캔버스 내 텍스처 좌표로 변환. 캔버스 중앙이 (0,0)에서 시작하므로 0.5 더해서 0~1 범위로 만든 뒤 텍스처 크기 곱함.
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out localPos);
        float x = (localPos.x / rectTransform.rect.width  + 0.5f) * canvasWidth;
        float y = (localPos.y / rectTransform.rect.height + 0.5f) * canvasHeight;
        return new Vector2(x, y);
    }

    Color GetActiveColor() // 현재 도구에 맞는 색상 반환. 지우개는 배경색 또는 투명색, 브러시는 알파 적용.
    {
        if (currentTool == DrawingTool.Eraser)
            return transparentBackground ? new Color(0f, 0f, 0f, 0f) : backgroundColor;
        Color c = brushColor;
        if (currentTool == DrawingTool.Brush)
            c.a *= brushAlpha;
        return c;
    }

    void PaintDot(int cx, int cy, int radius, Color color) // 중심(cx, cy)에서 반지름 radius인 원 범위 내 픽셀에 색상 color로 칠함. 원 내부 픽셀만 칠하도록 x^2+y^2<=r^2 조건 사용. 캔버스 밖 좌표는 무시.
    {
        for (int x = -  radius; x <= radius; x++)
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

    void PaintStroke(Vector2 from, Vector2 to) // 선분을 따라 점을 찍어서 그리는 방식. 간격이 너무 벌어지지 않도록 거리에 비례해서 점 개수 조절.
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

    public void SetTool(DrawingTool tool) // 도구 변경. UI 버튼에서 호출.
    {
        currentTool = tool;
        switch (tool)
        {
            case DrawingTool.Pen:
                brushColor = savedDrawColor;
                brushSize = penDefault;
                break;
            case DrawingTool.Brush:
                brushColor = savedDrawColor;
                brushSize = brushDefault;
                break;
            case DrawingTool.Eraser:
                brushSize = eraserDefault;
                break;
            case DrawingTool.Picker:
                break;
        }
    }

    public void SetBrushColor(Color color) //   색상 설정. UI 컬러 피커에서 호출.
    {
        savedDrawColor = color;
        if (currentTool != DrawingTool.Eraser)
            brushColor = color;
    }

    public void SetEraser() // 지우개 도구로 전환. UI 버튼에서 호출.
    {
        SetTool(DrawingTool.Eraser);
    }

    public void SetBrushSize(int size) // 굵기 설정. UI 슬라이더에서 호출.
    {
        brushSize = Mathf.Clamp(size, 1, 64);
    }

    public int GetDefaultThicknessForTool(DrawingTool tool) // 도구별 굵기 기본값 반환. UI 슬라이더 설정할 때 사용.
    {
        switch (tool)
        {
            case DrawingTool.Pen:    return penDefault;
            case DrawingTool.Brush:  return brushDefault;
            case DrawingTool.Eraser: return eraserDefault;
            default:                 return brushSize;
        }
    }

    public void GetThicknessRangeForTool(DrawingTool tool, out int min, out int max) // 현재 도구에 맞는 굵기 범위를 반환. UI 슬라이더 설정할 때 사용.
    {
        switch (tool)
        {
            case DrawingTool.Pen:    min = penMin;    max = penMax;    break;
            case DrawingTool.Brush:  min = brushMin;  max = brushMax;  break;
            case DrawingTool.Eraser: min = eraserMin; max = eraserMax; break;
            default:                 min = penMin;    max = penMax;    break;
        }
    }

    public int GetThicknessForCurrentTool() // 현재 도구에 맞는 굵기를 반환. UI 슬라이더 설정할 때 사용.
    {
        return brushSize;
    }

    public Color GetColorAtScreenPosition(Vector2 screenPos, Camera cam) // 캔버스 내에서 스크린 좌표에 해당하는 픽셀 색상을 반환. 캔버스 밖이면 투명색 반환.
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, cam, out localPos);
        int x = Mathf.RoundToInt((localPos.x / rectTransform.rect.width  + 0.5f) * canvasWidth);
        int y = Mathf.RoundToInt((localPos.y / rectTransform.rect.height + 0.5f) * canvasHeight);
        if (x < 0 || x >= canvasWidth || y < 0 || y >= canvasHeight) return Color.clear;
        return drawTexture.GetPixel(x, y);
    }

    public Texture2D GetDrawTexture() { return drawTexture; }

    public Texture2D GetFlattenedTextureForScoring() // 배경색과 섞인 최종 픽셀값을 반환. 투명 배경일 때는 투명 픽셀은 배경색으로 간주하여 섞음.
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
