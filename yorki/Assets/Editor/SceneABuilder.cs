using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

// SceneA 빌더.
// 위치/크기는 사용자가 직접 조정한 SceneA.unity 값을 그대로 코드에 반영함.
// 빌더 재실행 시 사용자 조정값을 덮어쓰지 않도록 주의.
public class SceneABuilder
{
    const string deskBasePath      = "Assets/Sprites/UI/SceneA/Desk/ui_fixed.png";
    const string finalRefPath      = "Assets/Sprites/UI/SceneA/Desk/ui_final_rgb_reference.png";
    const string sliderHandlePath  = "Assets/Sprites/UI/SceneA/Controls/slider_handle.png";

    const string penBasePath       = "Assets/Sprites/UI/SceneA/Tools/Pen/pen_base.png";
    const string penSelectingPath  = "Assets/Sprites/UI/SceneA/Tools/Pen/pen_selecting.png";
    const string penSelectedPath   = "Assets/Sprites/UI/SceneA/Tools/Pen/pen_selected.png";

    const string brushBasePath       = "Assets/Sprites/UI/SceneA/Tools/Brush/brush_base.png";
    const string brushSelectingPath  = "Assets/Sprites/UI/SceneA/Tools/Brush/brush_selecting.png";
    const string brushSelectedPath   = "Assets/Sprites/UI/SceneA/Tools/Brush/brush_selected.png";

    const string eraserBasePath       = "Assets/Sprites/UI/SceneA/Tools/Eraser/eraser_base.png";
    const string eraserSelectingPath  = "Assets/Sprites/UI/SceneA/Tools/Eraser/eraser_selecting.png";
    const string eraserSelectedPath   = "Assets/Sprites/UI/SceneA/Tools/Eraser/eraser_selected.png";

    const string pickerBasePath       = "Assets/Sprites/UI/SceneA/Tools/Picker/picker_base.png";
    const string pickerSelectingPath  = "Assets/Sprites/UI/SceneA/Tools/Picker/picker_selecting.png";
    const string pickerSelectedPath   = "Assets/Sprites/UI/SceneA/Tools/Picker/picker_selected.png";

    const string undoPath    = "Assets/Sprites/UI/SceneA/Actions/undo.png";
    const string unundoPath  = "Assets/Sprites/UI/SceneA/Actions/unundo.png";
    const string redoPath    = "Assets/Sprites/UI/SceneA/Actions/redo.png";
    const string unredoPath  = "Assets/Sprites/UI/SceneA/Actions/unredo.png";
    const string resetPath   = "Assets/Sprites/UI/SceneA/Actions/reset.png";
    const string submitPath  = "Assets/Sprites/UI/SceneA/Actions/submit.png";

    static readonly string[] uiSpritePaths = new string[]
    {
        deskBasePath,
        "Assets/Sprites/UI/SceneA/Desk/ui_final_reference.png",
        finalRefPath,
        sliderHandlePath,
        penBasePath, penSelectingPath, penSelectedPath,
        brushBasePath, brushSelectingPath, brushSelectedPath,
        eraserBasePath, eraserSelectingPath, eraserSelectedPath,
        pickerBasePath, pickerSelectingPath, pickerSelectedPath,
        undoPath, unundoPath, redoPath, unredoPath, resetPath, submitPath,
    };

    [MenuItem("Yorki/Build Scene A")]
    public static void Build()
    {
        BuildScene();
    }

    static void BuildScene()
    {
        AssetDatabase.Refresh();
        foreach (string path in uiSpritePaths)
            ConfigureSprite(path);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        cam.orthographic    = true;
        camGO.tag           = "MainCamera";

        // EventSystem
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // SceneCanvas
        var canvasGO = new GameObject("SceneCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // RightPanel
        var rightGO = new GameObject("RightPanel");
        rightGO.transform.SetParent(canvasGO.transform, false);
        var rightImg = rightGO.AddComponent<Image>();
        var rightGroup = rightGO.AddComponent<CanvasGroup>();
        rightImg.color = new Color(0f, 0f, 0f, 0f);
        rightImg.raycastTarget = false;
        rightGroup.alpha = 1f;
        var rightRT = rightGO.GetComponent<RectTransform>();
        rightRT.anchoredPosition = new Vector2(320, 0);
        rightRT.sizeDelta        = new Vector2(640, 720);

        // DeskBase — ui_fixed.png 베이스 (가장 밑바닥)
        var deskSprite = AssetDatabase.LoadAssetAtPath<Sprite>(deskBasePath);
        var deskGO = CreateImage(rightGO.transform, "DeskBase", deskSprite, Vector2.zero, new Vector2(640, 720), false);
        var deskImg = deskGO.GetComponent<Image>();
        deskImg.type = Image.Type.Simple;
        deskImg.preserveAspect = false;
        if (deskSprite == null)
        {
            deskImg.color = new Color(0.22f, 0.13f, 0.07f);
            Debug.LogWarning("[SceneABuilder] ui_fixed.png 없음 — 작업대 폴백 색상 사용");
        }

        // ReferenceOverlay_FinalRGB — 배치 비교용 비활성 레퍼런스
        var finalRefSprite = AssetDatabase.LoadAssetAtPath<Sprite>(finalRefPath);
        var refGO = CreateImage(rightGO.transform, "ReferenceOverlay_FinalRGB", finalRefSprite, Vector2.zero, new Vector2(640, 720), false);
        refGO.SetActive(false);

        // DrawingPaper / DrawingSurface — 사용자 조정값
        var paperGO = new GameObject("DrawingPaper");
        paperGO.transform.SetParent(rightGO.transform, false);
        var paperRT = paperGO.AddComponent<RectTransform>();
        paperRT.anchoredPosition = new Vector2(-18.18435f, 118f);
        paperRT.sizeDelta = new Vector2(330.915f, 406.5569f);

        var surfaceGO = new GameObject("DrawingSurface");
        surfaceGO.transform.SetParent(paperGO.transform, false);
        var surfaceRT = surfaceGO.AddComponent<RectTransform>();
        surfaceRT.anchorMin = Vector2.zero;
        surfaceRT.anchorMax = Vector2.one;
        surfaceRT.offsetMin = Vector2.zero;
        surfaceRT.offsetMax = Vector2.zero;
        var surfaceImg = surfaceGO.AddComponent<RawImage>();
        surfaceImg.color = Color.white;
        surfaceImg.raycastTarget = true;
        var drawingCanvas = surfaceGO.AddComponent<DrawingCanvas>();
        drawingCanvas.transparentBackground = true;
        drawingCanvas.backgroundColor = Color.white;
        drawingCanvas.brushColor = Color.black;
        drawingCanvas.brushSize = 8;

        // ─── Left Tools (사용자 조정값) ─────────────────────────
        var penBase       = AssetDatabase.LoadAssetAtPath<Sprite>(penBasePath);
        var penSelecting  = AssetDatabase.LoadAssetAtPath<Sprite>(penSelectingPath);
        var penSelected   = AssetDatabase.LoadAssetAtPath<Sprite>(penSelectedPath);
        var brushBase     = AssetDatabase.LoadAssetAtPath<Sprite>(brushBasePath);
        var brushSelecting= AssetDatabase.LoadAssetAtPath<Sprite>(brushSelectingPath);
        var brushSelected = AssetDatabase.LoadAssetAtPath<Sprite>(brushSelectedPath);
        var eraserBase    = AssetDatabase.LoadAssetAtPath<Sprite>(eraserBasePath);
        var eraserSelecting = AssetDatabase.LoadAssetAtPath<Sprite>(eraserSelectingPath);
        var eraserSelected= AssetDatabase.LoadAssetAtPath<Sprite>(eraserSelectedPath);
        var pickerBase    = AssetDatabase.LoadAssetAtPath<Sprite>(pickerBasePath);
        var pickerSelecting = AssetDatabase.LoadAssetAtPath<Sprite>(pickerSelectingPath);
        var pickerSelected= AssetDatabase.LoadAssetAtPath<Sprite>(pickerSelectedPath);

        var penGO    = CreateImage(rightGO.transform, "PenButton",    penSelected, new Vector2(-257f,   277.4f), new Vector2(95f, 105f),  true);
        var brushGO  = CreateImage(rightGO.transform, "BrushButton",  brushBase,   new Vector2(-257f,   180.1f), new Vector2(95f, 95.2f), true);
        var eraserGO = CreateImage(rightGO.transform, "EraserButton", eraserBase,  new Vector2(-257f,   84.1f),  new Vector2(95f, 97.3f), true);
        var pickerGO = CreateImage(rightGO.transform, "PickerButton", pickerBase,  new Vector2(-257.4f, -14.7f), new Vector2(95f, 97.2f), true);

        // ─── THICKNESS ────────────────────────────────────────────
        var sliderHandleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sliderHandlePath);

        var thicknessTrackGO = new GameObject("ThicknessTrack");
        thicknessTrackGO.transform.SetParent(rightGO.transform, false);
        var trackImg = thicknessTrackGO.AddComponent<Image>();
        trackImg.color = new Color(0, 0, 0, 0);
        trackImg.raycastTarget = true;
        var trackRT = thicknessTrackGO.GetComponent<RectTransform>();
        trackRT.anchoredPosition = new Vector2(180f, 79.85f);
        trackRT.sizeDelta = new Vector2(40f, 220f);

        var sliderHandleGO = new GameObject("SliderHandle");
        sliderHandleGO.transform.SetParent(thicknessTrackGO.transform, false);
        var handleImg = sliderHandleGO.AddComponent<Image>();
        handleImg.sprite = sliderHandleSprite;
        handleImg.preserveAspect = true;
        handleImg.raycastTarget = true;
        var handleRT = sliderHandleGO.GetComponent<RectTransform>();
        // 사용자가 Unity에서 맞춘 SliderHandle 기본 위치/크기.
        handleRT.anchoredPosition = new Vector2(14.822153f, 4.98325f);
        handleRT.sizeDelta = new Vector2(34.9842f, 28.5703f);

        var sliderHandleScript = sliderHandleGO.AddComponent<ThicknessSliderHandle>();
        sliderHandleScript.track  = trackRT;
        sliderHandleScript.handle = handleRT;
        sliderHandleScript.centerY = 4.98325f;
        sliderHandleScript.minY = -70f;
        sliderHandleScript.maxY = 70f;

        // THICKNESS 라벨 ("8 px") — 사용자 조정값
        var thicknessTextGO = new GameObject("ThicknessText");
        thicknessTextGO.transform.SetParent(rightGO.transform, false);
        var thicknessText = thicknessTextGO.AddComponent<Text>();
        thicknessText.text = "8 px";
        thicknessText.font = YorkiEditorAssets.LoadUIFont();
        thicknessText.fontSize = 18;
        thicknessText.color = new Color(0.20f, 0.12f, 0.06f);
        thicknessText.alignment = TextAnchor.MiddleCenter;
        thicknessText.raycastTarget = false;
        var thicknessTextRT = thicknessTextGO.GetComponent<RectTransform>();
        thicknessTextRT.anchoredPosition = new Vector2(242.6181f, 210.79393f);
        thicknessTextRT.sizeDelta = new Vector2(63.6871f, 28.2122f);

        // PreviewDot — 사용자 조정값
        var previewDotGO = new GameObject("PreviewDot");
        previewDotGO.transform.SetParent(rightGO.transform, false);
        var previewDotImg = previewDotGO.AddComponent<Image>();
        previewDotImg.color = new Color(0f, 0f, 0f, 0f); // 시각 효과는 ui_fixed.png에 이미 포함
        previewDotImg.raycastTarget = false;
        var previewDotRT = previewDotGO.GetComponent<RectTransform>();
        previewDotRT.anchoredPosition = new Vector2(252f, 175f);
        previewDotRT.sizeDelta = new Vector2(20f, 20f);

        // ─── Undo / Redo / Reset / Submit (사용자 조정값) ─────────
        var undoSprite   = AssetDatabase.LoadAssetAtPath<Sprite>(undoPath);
        var unundoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(unundoPath);
        var redoSprite   = AssetDatabase.LoadAssetAtPath<Sprite>(redoPath);
        var unredoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(unredoPath);
        var resetSprite  = AssetDatabase.LoadAssetAtPath<Sprite>(resetPath);
        var submitSprite = AssetDatabase.LoadAssetAtPath<Sprite>(submitPath);

        var undoGO   = CreateImage(rightGO.transform, "UndoButton",   undoSprite,   new Vector2(198.9f, -27f),    new Vector2(67.2f,  86.8f),  true);
        var redoGO   = CreateImage(rightGO.transform, "RedoButton",   redoSprite,   new Vector2(265.6f, -26.6f),  new Vector2(67.2f,  86.8f),  true);
        var resetGO  = CreateImage(rightGO.transform, "ResetButton",  resetSprite,  new Vector2(220.3f, -153f),   new Vector2(175.3f, 94.1f),  true);
        var submitGO = CreateImage(rightGO.transform, "SubmitButton", submitSprite, new Vector2(219.9f, -264.2f), new Vector2(163.8f, 112.5f), true);

        // ─── PALETTE 8칸 (Step 6) ───────────────────────────────
        // ui 고정.png 검은 박스 위치 분석값 기반.
        // 박스 중심 unity_x: ui 초안 최종.png 색상 추출 위치와 일치.
        var palettePanelGO = new GameObject("PalettePanel");
        palettePanelGO.transform.SetParent(rightGO.transform, false);
        var palettePanelRT = palettePanelGO.AddComponent<RectTransform>();
        palettePanelRT.anchorMin = new Vector2(0.5f, 0.5f);
        palettePanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        palettePanelRT.anchoredPosition = Vector2.zero;
        palettePanelRT.sizeDelta = new Vector2(640f, 720f);

        float[] paletteCenterX = { -268.2f, -216.7f, -165.3f, -113.5f, -61.7f, -9.9f, 41.8f, 93.6f };
        float paletteCenterY = -158f;
        Vector2 paletteSize = new Vector2(34f, 40f);
        Color[] paletteDefaultColors = new Color[]
        {
            new Color32(226, 157, 104, 255), // 살색
            new Color32(183, 115,  64, 255), // 밝은 갈색
            new Color32(127,  69,  38, 255), // 중간 갈색
            new Color32( 66,  37,  23, 255), // 어두운 갈색
            new Color32(106,  92,  79, 255), // 회갈색
            new Color32(185,  54,  39, 255), // 빨강
            new Color32(230, 194, 148, 255), // 크림색
            new Color32( 33,  45,  69, 255), // 남색
        };

        Image[] paletteSlots = new Image[8];
        for (int i = 0; i < 8; i++)
        {
            var slotGO = new GameObject("PaletteSlot_" + i);
            slotGO.transform.SetParent(palettePanelGO.transform, false);
            var slotImg = slotGO.AddComponent<Image>();
            slotImg.color = paletteDefaultColors[i];
            slotImg.raycastTarget = true;
            var slotRT = slotGO.GetComponent<RectTransform>();
            slotRT.anchoredPosition = new Vector2(paletteCenterX[i], paletteCenterY);
            slotRT.sizeDelta = paletteSize;
            paletteSlots[i] = slotImg;
        }

        // ─── COLOR / RGB 컬러 박스 (스포이드 아이콘 제외) ───────
        // ui 초안 최종.png 기준 하단 COLOR 박스 내부 요소.
        var colorPanelGO = new GameObject("ColorPanel");
        colorPanelGO.transform.SetParent(rightGO.transform, false);
        var colorPanelRT = colorPanelGO.AddComponent<RectTransform>();
        colorPanelRT.anchorMin = new Vector2(0.5f, 0.5f);
        colorPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        colorPanelRT.anchoredPosition = Vector2.zero;
        colorPanelRT.sizeDelta = new Vector2(640f, 720f);

        var svArea = CreateRawImage(colorPanelGO.transform, "SaturationValueArea", new Vector2(-216f, -279f), new Vector2(143f, 100f), true);
        var hueBar = CreateRawImage(colorPanelGO.transform, "HueBar", new Vector2(-120f, -279f), new Vector2(22f, 100f), true);

        var labelColor = new Color(0.96f, 0.88f, 0.64f);
        CreateText(colorPanelGO.transform, "RLabel", "R", new Vector2(-76f, -244f), new Vector2(22f, 28f), 22, labelColor, TextAnchor.MiddleCenter);
        CreateText(colorPanelGO.transform, "GLabel", "G", new Vector2(-76f, -280f), new Vector2(22f, 28f), 22, labelColor, TextAnchor.MiddleCenter);
        CreateText(colorPanelGO.transform, "BLabel", "B", new Vector2(-76f, -316f), new Vector2(22f, 28f), 22, labelColor, TextAnchor.MiddleCenter);

        var rInput = CreateInputField(colorPanelGO.transform, "RValueInput", new Vector2(-22f, -244f), new Vector2(76f, 28f));
        var gInput = CreateInputField(colorPanelGO.transform, "GValueInput", new Vector2(-22f, -280f), new Vector2(76f, 28f));
        var bInput = CreateInputField(colorPanelGO.transform, "BValueInput", new Vector2(-22f, -316f), new Vector2(76f, 28f));

        var colorPreviewGO = CreateImage(colorPanelGO.transform, "ColorPreview", null, new Vector2(73f, -279f), new Vector2(70f, 88f), false);
        var colorPreview = colorPreviewGO.GetComponent<Image>();
        colorPreview.color = paletteDefaultColors[0];
        var colorPreviewOutline = colorPreviewGO.AddComponent<Outline>();
        colorPreviewOutline.effectColor = new Color(0.12f, 0.07f, 0.03f, 1f);
        colorPreviewOutline.effectDistance = new Vector2(3f, -3f);

        // ─── SceneADrawingUIController ───────────────────────────
        var controllerGO = new GameObject("SceneADrawingUIController");
        controllerGO.transform.SetParent(rightGO.transform, false);
        var controller = controllerGO.AddComponent<SceneADrawingUIController>();
        controller.drawingCanvas = drawingCanvas;

        controller.penButton    = penGO.GetComponent<Image>();
        controller.brushButton  = brushGO.GetComponent<Image>();
        controller.eraserButton = eraserGO.GetComponent<Image>();
        controller.pickerButton = pickerGO.GetComponent<Image>();

        controller.penBase      = penBase;     controller.penSelecting    = penSelecting;    controller.penSelected     = penSelected;
        controller.brushBase    = brushBase;   controller.brushSelecting  = brushSelecting;  controller.brushSelected   = brushSelected;
        controller.eraserBase   = eraserBase;  controller.eraserSelecting = eraserSelecting; controller.eraserSelected  = eraserSelected;
        controller.pickerBase   = pickerBase;  controller.pickerSelecting = pickerSelecting; controller.pickerSelected  = pickerSelected;

        controller.thicknessSlider = sliderHandleScript;
        controller.thicknessText   = thicknessText;
        controller.previewDot      = previewDotImg;

        controller.undoButton   = undoGO.GetComponent<Image>();
        controller.redoButton   = redoGO.GetComponent<Image>();
        controller.resetButton  = resetGO.GetComponent<Image>();
        controller.submitButton = submitGO.GetComponent<Image>();
        controller.undoActive   = undoSprite;
        controller.undoInactive = unundoSprite;
        controller.redoActive   = redoSprite;
        controller.redoInactive = unredoSprite;
        controller.resetSprite  = resetSprite;

        controller.paletteSlots = paletteSlots;

        controller.saturationValueArea = svArea;
        controller.hueBar              = hueBar;
        controller.rInput              = rInput;
        controller.gInput              = gInput;
        controller.bInput              = bInput;
        controller.colorPreview        = colorPreview;

        // SceneTransition
        var transitionGO = new GameObject("SceneTransition");
        transitionGO.AddComponent<SceneTransition>();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SceneA.unity");
        Debug.Log("[SceneABuilder] SceneA 생성 완료 — RGB 컬러 박스 + Submit 연결 + Step 6 UI 반영");
    }

    static GameObject CreateImage(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, bool raycastTarget)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = Color.white;
        img.raycastTarget = raycastTarget;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return go;
    }

    static RawImage CreateRawImage(Transform parent, string name, Vector2 position, Vector2 size, bool raycastTarget)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<RawImage>();
        img.color = Color.white;
        img.raycastTarget = raycastTarget;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return img;
    }

    static Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, Color color, TextAnchor alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.text = value;
        text.font = YorkiEditorAssets.LoadUIFont();
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return text;
    }

    static InputField CreateInputField(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.11f, 0.07f, 0.96f);
        bg.raycastTarget = true;

        var input = go.AddComponent<InputField>();
        input.targetGraphic = bg;
        input.contentType = InputField.ContentType.IntegerNumber;
        input.characterLimit = 3;
        input.text = "0";

        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var text = textGO.AddComponent<Text>();
        text.font = YorkiEditorAssets.LoadUIFont();
        text.fontSize = 20;
        text.color = new Color(0.96f, 0.88f, 0.64f);
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        input.textComponent = text;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0.08f, 0.04f, 0.02f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        return input;
    }

    static void ConfigureSprite(string path)
    {
        YorkiEditorAssets.ConfigureSprite(path, "[SceneABuilder] UI 스프라이트 못 찾음: ");
    }
}
