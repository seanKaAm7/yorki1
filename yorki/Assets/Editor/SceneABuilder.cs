using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

// 캔버스 모드 씬 빌더.
// 손님·배경·대사창은 PersistentBootstrap(영속 객체)가 보유하므로 여기선 만들지 않음.
// 작업대(RightPanel) + 카메라/캔버스만 생성.
//
// 베이스: ui_fixed.png (작업대 + 종이 + 빈 PALETTE/COLOR 박스 + THICKNESS 판)
// 위에 얹힘: 좌측 도구 4종, THICKNESS 슬라이더 손잡이, Undo/Redo/Reset/Submit 버튼.
// 결과적으로 ui 초안 최종.png 모습이 되어야 함.
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

        // DeskBase — ui_fixed.png 베이스 작업대 (가장 밑바닥)
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

        // DrawingPaper — 종이 안쪽 드로잉 레이어
        var paperGO = new GameObject("DrawingPaper");
        paperGO.transform.SetParent(rightGO.transform, false);
        var paperRT = paperGO.AddComponent<RectTransform>();
        paperRT.anchoredPosition = new Vector2(-18f, 118f);
        paperRT.sizeDelta = new Vector2(304f, 366f);

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

        // ─── Left Tools (좌측 4종 도구) ─────────────────────────────
        // 추정 위치 — ui 초안 최종.png 기준. ReferenceOverlay 켜고 미세 조정 예정.
        var toolSize = new Vector2(80, 80);
        float toolX = -262f;
        float toolStartY = 215f;
        float toolGap   = 84f;

        var penSelected   = AssetDatabase.LoadAssetAtPath<Sprite>(penSelectedPath);
        var penBase       = AssetDatabase.LoadAssetAtPath<Sprite>(penBasePath);
        var brushBase     = AssetDatabase.LoadAssetAtPath<Sprite>(brushBasePath);
        var brushSelected = AssetDatabase.LoadAssetAtPath<Sprite>(brushSelectedPath);
        var eraserBase    = AssetDatabase.LoadAssetAtPath<Sprite>(eraserBasePath);
        var eraserSelected= AssetDatabase.LoadAssetAtPath<Sprite>(eraserSelectedPath);
        var pickerBase    = AssetDatabase.LoadAssetAtPath<Sprite>(pickerBasePath);
        var pickerSelected= AssetDatabase.LoadAssetAtPath<Sprite>(pickerSelectedPath);

        var penGO    = CreateImage(rightGO.transform, "PenButton",    penSelected,   new Vector2(toolX, toolStartY - toolGap * 0), toolSize, true);
        var brushGO  = CreateImage(rightGO.transform, "BrushButton",  brushBase,     new Vector2(toolX, toolStartY - toolGap * 1), toolSize, true);
        var eraserGO = CreateImage(rightGO.transform, "EraserButton", eraserBase,    new Vector2(toolX, toolStartY - toolGap * 2), toolSize, true);
        var pickerGO = CreateImage(rightGO.transform, "PickerButton", pickerBase,    new Vector2(toolX, toolStartY - toolGap * 3), toolSize, true);

        // ─── THICKNESS 슬라이더 ──────────────────────────────────────
        // 검은 세로 홈 위에 트랙(투명 입력 영역) + 손잡이 얹기.
        var sliderHandleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sliderHandlePath);

        var thicknessTrackGO = new GameObject("ThicknessTrack");
        thicknessTrackGO.transform.SetParent(rightGO.transform, false);
        var trackImg = thicknessTrackGO.AddComponent<Image>();
        trackImg.color = new Color(0, 0, 0, 0);
        trackImg.raycastTarget = true;
        var trackRT = thicknessTrackGO.GetComponent<RectTransform>();
        trackRT.anchoredPosition = new Vector2(180f, 200f);
        trackRT.sizeDelta = new Vector2(40f, 220f);

        var sliderHandleGO = new GameObject("SliderHandle");
        sliderHandleGO.transform.SetParent(thicknessTrackGO.transform, false);
        var handleImg = sliderHandleGO.AddComponent<Image>();
        handleImg.sprite = sliderHandleSprite;
        handleImg.preserveAspect = true;
        handleImg.raycastTarget = true;
        var handleRT = sliderHandleGO.GetComponent<RectTransform>();
        handleRT.anchoredPosition = new Vector2(0, 0);
        handleRT.sizeDelta = new Vector2(56f, 40f);

        var sliderHandleScript = sliderHandleGO.AddComponent<ThicknessSliderHandle>();
        sliderHandleScript.track  = trackRT;
        sliderHandleScript.handle = handleRT;
        sliderHandleScript.minY = -100f;
        sliderHandleScript.maxY = 100f;

        // THICKNESS 라벨 ("8 px")
        var thicknessTextGO = new GameObject("ThicknessText");
        thicknessTextGO.transform.SetParent(rightGO.transform, false);
        var thicknessText = thicknessTextGO.AddComponent<Text>();
        thicknessText.text = "8 px";
        thicknessText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        thicknessText.fontSize = 18;
        thicknessText.color = new Color(0.20f, 0.12f, 0.06f);
        thicknessText.alignment = TextAnchor.MiddleCenter;
        thicknessText.raycastTarget = false;
        var thicknessTextRT = thicknessTextGO.GetComponent<RectTransform>();
        thicknessTextRT.anchoredPosition = new Vector2(212f, 268f);
        thicknessTextRT.sizeDelta = new Vector2(60f, 26f);

        // PreviewDot — THICKNESS 박스 내부 동그라미 (크기 미리보기)
        var previewDotGO = new GameObject("PreviewDot");
        previewDotGO.transform.SetParent(rightGO.transform, false);
        var previewDotImg = previewDotGO.AddComponent<Image>();
        previewDotImg.color = new Color(0f, 0f, 0f, 0f); // ui_fixed.png에 이미 검은 동그라미 그려져 있어 비활성 효과
        previewDotImg.raycastTarget = false;
        var previewDotRT = previewDotGO.GetComponent<RectTransform>();
        previewDotRT.anchoredPosition = new Vector2(252f, 175f);
        previewDotRT.sizeDelta = new Vector2(20f, 20f);

        // ─── Undo / Redo 버튼 ────────────────────────────────────────
        var undoSprite   = AssetDatabase.LoadAssetAtPath<Sprite>(undoPath);
        var unundoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(unundoPath);
        var redoSprite   = AssetDatabase.LoadAssetAtPath<Sprite>(redoPath);
        var unredoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(unredoPath);
        var resetSprite  = AssetDatabase.LoadAssetAtPath<Sprite>(resetPath);
        var submitSprite = AssetDatabase.LoadAssetAtPath<Sprite>(submitPath);

        var undoGO = CreateImage(rightGO.transform, "UndoButton", undoSprite, new Vector2(212f, 25f), new Vector2(60f, 80f), true);
        var redoGO = CreateImage(rightGO.transform, "RedoButton", redoSprite, new Vector2(280f, 25f), new Vector2(60f, 80f), true);
        var resetGO = CreateImage(rightGO.transform, "ResetButton", resetSprite, new Vector2(248f, -88f), new Vector2(135f, 58f), true);
        var submitGO = CreateImage(rightGO.transform, "SubmitButton", submitSprite, new Vector2(248f, -198f), new Vector2(150f, 100f), true);

        // ─── SceneADrawingUIController ───────────────────────────────
        var controllerGO = new GameObject("SceneADrawingUIController");
        controllerGO.transform.SetParent(rightGO.transform, false);
        var controller = controllerGO.AddComponent<SceneADrawingUIController>();
        controller.drawingCanvas = drawingCanvas;
        controller.penButton    = penGO.GetComponent<Image>();
        controller.brushButton  = brushGO.GetComponent<Image>();
        controller.eraserButton = eraserGO.GetComponent<Image>();
        controller.pickerButton = pickerGO.GetComponent<Image>();
        controller.penBase     = penBase;     controller.penSelected     = penSelected;
        controller.brushBase   = brushBase;   controller.brushSelected   = brushSelected;
        controller.eraserBase  = eraserBase;  controller.eraserSelected  = eraserSelected;
        controller.pickerBase  = pickerBase;  controller.pickerSelected  = pickerSelected;
        controller.thicknessSlider = sliderHandleScript;
        controller.thicknessText   = thicknessText;
        controller.previewDot      = previewDotImg;
        controller.minThickness = 2;
        controller.maxThickness = 24;
        controller.undoButton  = undoGO.GetComponent<Image>();
        controller.redoButton  = redoGO.GetComponent<Image>();
        controller.resetButton = resetGO.GetComponent<Image>();
        controller.undoActive   = undoSprite;
        controller.undoInactive = unundoSprite;
        controller.redoActive   = redoSprite;
        controller.redoInactive = unredoSprite;
        controller.resetSprite  = resetSprite;

        // SceneTransition + 임시 G/B 입력
        var transitionGO = new GameObject("SceneTransition");
        transitionGO.AddComponent<SceneTransition>();
        var testInputGO = new GameObject("SceneATestTransitionInput");
        testInputGO.AddComponent<SceneATestTransitionInput>();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SceneA.unity");
        Debug.Log("[SceneABuilder] SceneA 생성 완료 — 좌측 도구/THICKNESS/Undo·Redo·Reset·Submit 2차 1차 시도");
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

    static void ConfigureSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("[SceneABuilder] UI 스프라이트 못 찾음: " + path);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }
}
