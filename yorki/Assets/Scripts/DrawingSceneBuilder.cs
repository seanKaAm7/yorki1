using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class DrawingSceneBuilder : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Yorki/Build Drawing Scene")]
    public static void BuildScene()
    {
        // --- Canvas ---
        GameObject canvasGO = new GameObject("DrawingUI");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // --- Background ---
        CreatePanel(canvasGO.transform, "Background", Color.black, Vector2.zero, new Vector2(1920, 1080));

        // --- Customer Image (왼쪽) ---
        GameObject customerGO = new GameObject("CustomerImage");
        customerGO.transform.SetParent(canvasGO.transform, false);
        RawImage customerImg = customerGO.AddComponent<RawImage>();
        RectTransform customerRect = customerGO.GetComponent<RectTransform>();
        customerRect.sizeDelta        = new Vector2(420, 540);
        customerRect.anchoredPosition = new Vector2(-380, 50);

        Texture2D sampleTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/토미에 샘플.png");
        if (sampleTex != null)
            customerImg.texture = sampleTex;
        else
            customerImg.color = new Color(0.8f, 0.75f, 0.65f);

        // --- Drawing Panel (오른쪽) ---
        GameObject drawPanelGO = new GameObject("DrawingPanel");
        drawPanelGO.transform.SetParent(canvasGO.transform, false);
        drawPanelGO.AddComponent<RawImage>().color = Color.white;
        RectTransform drawRect = drawPanelGO.GetComponent<RectTransform>();
        drawRect.sizeDelta        = new Vector2(480, 540);
        drawRect.anchoredPosition = new Vector2(150, 50);
        drawPanelGO.AddComponent<DrawingCanvas>();

        // --- Toolbar (하단) ---
        // 버튼 배치만. 리스너는 DrawingToolbar.Start()에서 런타임에 연결됨
        GameObject toolbar = CreatePanel(canvasGO.transform, "Toolbar",
            new Color(0.15f, 0.1f, 0.08f), new Vector2(0, -460), new Vector2(1200, 100));

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
            GameObject colorBtn = new GameObject("Color_" + i);
            colorBtn.transform.SetParent(toolbar.transform, false);
            colorBtn.AddComponent<Image>().color = palette[i];
            colorBtn.AddComponent<Button>();
            RectTransform r = colorBtn.GetComponent<RectTransform>();
            r.sizeDelta        = new Vector2(52, 52);
            r.anchoredPosition = new Vector2(-500f + i * 60, 0);
        }

        CreateTextButton(toolbar.transform, "Eraser", "지우개",      new Vector2(240, 0), new Vector2(90,  52));
        CreateTextButton(toolbar.transform, "Clear",  "전체 지우기", new Vector2(340, 0), new Vector2(110, 52));
        CreateTextButton(toolbar.transform, "Submit", "제출",         new Vector2(460, 0), new Vector2(90,  52), new Color(0.3f, 0.7f, 0.3f));

        toolbar.AddComponent<DrawingToolbar>();

        Debug.Log("[DrawingSceneBuilder] Drawing Scene 빌드 완료!");
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta        = size;
        rect.anchoredPosition = pos;
        return go;
    }

    static GameObject CreateTextButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, Color bgColor = default)
    {
        if (bgColor == default) bgColor = new Color(0.3f, 0.25f, 0.2f);

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = bgColor;
        go.AddComponent<Button>();
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta        = size;
        rect.anchoredPosition = pos;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text      = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color     = Color.white;
        text.fontSize  = 18;
        text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return go;
    }
#endif
}
