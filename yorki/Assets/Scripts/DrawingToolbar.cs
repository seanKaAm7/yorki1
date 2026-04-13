using UnityEngine;
using UnityEngine.UI;

public class DrawingToolbar : MonoBehaviour
{
    public DrawingCanvas drawingCanvas;

    void Start()
    {
        if (drawingCanvas == null)
            drawingCanvas = Object.FindAnyObjectByType<DrawingCanvas>();

        // 색상 버튼
        Color[] palette = new Color[]
        {
            Color.black, new Color(0.2f,0.2f,0.2f), Color.white,
            new Color(0.8f,0.2f,0.2f), new Color(0.9f,0.5f,0.1f),
            new Color(0.9f,0.85f,0.2f), new Color(0.2f,0.6f,0.2f),
            new Color(0.2f,0.4f,0.85f), new Color(0.5f,0.2f,0.7f),
            new Color(0.9f,0.6f,0.7f), new Color(0.6f,0.4f,0.2f),
            new Color(0.95f,0.88f,0.75f)
        };

        for (int i = 0; i < palette.Length; i++)
        {
            Color c = palette[i];
            Button btn = transform.Find("Color_" + i)?.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => drawingCanvas.SetBrushColor(c));
            }
        }

        // 지우개
        Button eraser = transform.Find("Eraser")?.GetComponent<Button>();
        if (eraser != null)
        {
            eraser.onClick.RemoveAllListeners();
            eraser.onClick.AddListener(() => drawingCanvas.SetEraser());
        }

        // 전체 지우기
        Button clear = transform.Find("Clear")?.GetComponent<Button>();
        if (clear != null)
        {
            clear.onClick.RemoveAllListeners();
            clear.onClick.AddListener(() => drawingCanvas.ClearCanvas());
        }

        // 제출
        Button submit = transform.Find("Submit")?.GetComponent<Button>();
        if (submit != null)
        {
            submit.onClick.RemoveAllListeners();
            submit.onClick.AddListener(() => GameManager.Instance?.OnSubmit());
        }
    }
}
