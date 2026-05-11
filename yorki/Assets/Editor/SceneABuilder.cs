using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

// 캔버스 모드 씬 빌더.
// 손님·배경·대사창은 PersistentBootstrap(영속 객체)가 보유하므로 여기선 만들지 않음.
// 작업대(RightPanel) + 카메라/캔버스만 생성.
public class SceneABuilder
{
    const string deskBasePath      = "Assets/Sprites/UI/SceneA/Desk/ui_fixed.png";
    const string finalRefPath      = "Assets/Sprites/UI/SceneA/Desk/ui_final_rgb_reference.png";
    static readonly string[] uiSpritePaths = new string[]
    {
        deskBasePath,
        "Assets/Sprites/UI/SceneA/Desk/ui_final_reference.png",
        finalRefPath,
        "Assets/Sprites/UI/SceneA/Controls/slider_handle.png",
        "Assets/Sprites/UI/SceneA/Tools/Pen/pen_base.png",
        "Assets/Sprites/UI/SceneA/Tools/Pen/pen_selecting.png",
        "Assets/Sprites/UI/SceneA/Tools/Pen/pen_selected.png",
        "Assets/Sprites/UI/SceneA/Tools/Brush/brush_base.png",
        "Assets/Sprites/UI/SceneA/Tools/Brush/brush_selecting.png",
        "Assets/Sprites/UI/SceneA/Tools/Brush/brush_selected.png",
        "Assets/Sprites/UI/SceneA/Tools/Eraser/eraser_base.png",
        "Assets/Sprites/UI/SceneA/Tools/Eraser/eraser_selecting.png",
        "Assets/Sprites/UI/SceneA/Tools/Eraser/eraser_selected.png",
        "Assets/Sprites/UI/SceneA/Tools/Picker/picker_base.png",
        "Assets/Sprites/UI/SceneA/Tools/Picker/picker_selecting.png",
        "Assets/Sprites/UI/SceneA/Tools/Picker/picker_selected.png",
        "Assets/Sprites/UI/SceneA/Actions/undo.png",
        "Assets/Sprites/UI/SceneA/Actions/unundo.png",
        "Assets/Sprites/UI/SceneA/Actions/redo.png",
        "Assets/Sprites/UI/SceneA/Actions/unredo.png",
        "Assets/Sprites/UI/SceneA/Actions/reset.png",
        "Assets/Sprites/UI/SceneA/Actions/submit.png",
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

        // SceneCanvas — 영속 캔버스보다 위(작업대가 영속 손님 위에 안 가려지게는 X,
        // 우측 패널만 우측 절반을 덮으므로 sortingOrder만 높이면 됨)
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

        // RightPanel — 우측 640x720 작업대 기준 영역
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

        // DeskBase — Play Ref/UI 초안/전체/ui 고정.png 기반 고정 작업대
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

        // ReferenceOverlay_FinalRGB — 배치 비교용 비활성 레퍼런스. 필요할 때 Hierarchy에서 켜서 비교.
        var finalRefSprite = AssetDatabase.LoadAssetAtPath<Sprite>(finalRefPath);
        var refGO = CreateImage(rightGO.transform, "ReferenceOverlay_FinalRGB", finalRefSprite, Vector2.zero, new Vector2(640, 720), false);
        refGO.SetActive(false);

        // DrawingPaper — ui 초안 최종 기준 종이 내부 드로잉 레이어.
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

        // SceneTransition — TalkScene에서 넘어온 싱글턴이 없을 때 직접 실행용 폴백
        var transitionGO = new GameObject("SceneTransition");
        transitionGO.AddComponent<SceneTransition>();

        // 임시 테스트 입력 — SceneTransition과 분리해야 영속 전환 싱글턴 중복 제거 때 같이 사라지지 않음
        var testInputGO = new GameObject("SceneATestTransitionInput");
        testInputGO.AddComponent<SceneATestTransitionInput>();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SceneA.unity");
        Debug.Log("[SceneABuilder] SceneA 생성 완료 — ui 고정 작업대 + 종이 드로잉 레이어 1차 반영");
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
