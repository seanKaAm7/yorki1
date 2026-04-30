using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

// 대화 씬 빌더 — 풀화면 배경 + 손님(중앙) + 대사창.
// 이 씬에 영속 객체(PersistentBootstrap + CustomerStage + Customer + Background)도 함께 만들어둔다.
// 게임 시작 시 TalkScene이 먼저 로드되면, PersistentBootstrap.Awake에서 DontDestroyOnLoad 처리.
public class TalkSceneBuilder
{
    // 배경 / 대사창
    const string bgPath        = "Assets/Sprites/SceneA/BG_Street.png";
    const string dlgBoxPath    = "Assets/Sprites/SceneA/DialogueBox.png";

    // 9-slice border
    const int dlgBorderL = 80, dlgBorderR = 80, dlgBorderT = 100, dlgBorderB = 30;

    // 손님 컷 (Neutral 입 단계 4종 + 그 외 감정)
    const string neutralIdlePath = "Assets/Sprites/SceneA/Customer_Neutral_Idle.png";
    const string neutralTalkPath = "Assets/Sprites/SceneA/Customer_Neutral_Talk.png";
    const string talk1Path       = "Assets/Sprites/SceneA/Customer_Talk1.png";
    const string talk2Path       = "Assets/Sprites/SceneA/Customer_Talk2.png";
    const string happyIdlePath   = "Assets/Sprites/SceneA/Customer_Happy_Idle.png";
    const string surprisedPath   = "Assets/Sprites/SceneA/Customer_Surprised.png";
    const string gestureIdlePath = "Assets/Sprites/SceneA/Customer_Gesture_Idle.png";
    const string gestureTalkPath = "Assets/Sprites/SceneA/Customer_Gesture_Talk.png";

    [MenuItem("Yorki/Build Talk Scene")]
    public static void Build()
    {
        SetupSprites();
        AssetDatabase.Refresh();
        BuildScene();
    }

    static void SetupSprites()
    {
        string[] paths = {
            bgPath, dlgBoxPath,
            neutralIdlePath, neutralTalkPath, talk1Path, talk2Path,
            happyIdlePath, surprisedPath, gestureIdlePath, gestureTalkPath
        };
        foreach (var p in paths)
        {
            var imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp == null) { Debug.LogWarning("[TalkSceneBuilder] 못 찾음: " + p); continue; }
            imp.textureType         = TextureImporterType.Sprite;
            imp.filterMode          = FilterMode.Point;
            imp.textureCompression  = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled       = false;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
        }
        // DialogueBox — 9-slice 설정
        var dlgImp = AssetImporter.GetAtPath(dlgBoxPath) as TextureImporter;
        if (dlgImp != null)
        {
            dlgImp.textureType         = TextureImporterType.Sprite;
            dlgImp.spriteImportMode    = SpriteImportMode.Single;
            dlgImp.filterMode          = FilterMode.Bilinear;
            dlgImp.textureCompression  = TextureImporterCompression.Uncompressed;
            dlgImp.mipmapEnabled       = false;
            dlgImp.alphaIsTransparency = true;
            dlgImp.spriteBorder        = new Vector4(dlgBorderL, dlgBorderB, dlgBorderR, dlgBorderT);
            dlgImp.SaveAndReimport();
        }
        Debug.Log("[TalkSceneBuilder] 스프라이트 설정 완료");
    }

    static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera & EventSystem ──────────────────────────────
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        cam.orthographic    = true;
        camGO.tag           = "MainCamera";

        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── PersistentBootstrap (영속 객체) ────────────────────
        var bootGO = new GameObject("PersistentBootstrap");
        var boot   = bootGO.AddComponent<PersistentBootstrap>();

        // PersistentCanvas (sortingOrder 0 — 씬 캔버스보다 뒤)
        var pCanvasGO = new GameObject("PersistentCanvas");
        pCanvasGO.transform.SetParent(bootGO.transform, false);
        var pCanvas = pCanvasGO.AddComponent<Canvas>();
        pCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        pCanvas.sortingOrder = 0;
        var pScaler = pCanvasGO.AddComponent<CanvasScaler>();
        pScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        pScaler.referenceResolution = new Vector2(1280, 720);
        pScaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        pScaler.matchWidthOrHeight  = 0.5f;
        pCanvasGO.AddComponent<GraphicRaycaster>();

        // CustomerStage (빈 부모) — TalkScene 기본 위치 (0, 0)
        var stageGO = new GameObject("CustomerStage", typeof(RectTransform));
        stageGO.transform.SetParent(pCanvasGO.transform, false);
        var stageRT = stageGO.GetComponent<RectTransform>();
        stageRT.anchorMin        = new Vector2(0.5f, 0.5f);
        stageRT.anchorMax        = new Vector2(0.5f, 0.5f);
        stageRT.pivot            = new Vector2(0.5f, 0.5f);
        stageRT.sizeDelta        = Vector2.zero;
        stageRT.anchoredPosition = Vector2.zero;

        // Background (CustomerStage 자식)
        var bgGO  = new GameObject("Background");
        bgGO.transform.SetParent(stageGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.sprite         = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
        bgImg.preserveAspect = true;
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchoredPosition = Vector2.zero;
        bgRT.sizeDelta        = new Vector2(1330, 720);

        // Customer (CustomerStage 자식)
        var customerGO  = new GameObject("Customer");
        customerGO.transform.SetParent(stageGO.transform, false);
        var customerImg = customerGO.AddComponent<Image>();
        customerImg.sprite         = AssetDatabase.LoadAssetAtPath<Sprite>(neutralIdlePath);
        customerImg.preserveAspect = true;
        var customerRT = customerGO.GetComponent<RectTransform>();
        customerRT.anchoredPosition = new Vector2(0, -50);
        customerRT.sizeDelta        = new Vector2(320, 320);

        customerGO.AddComponent<FadeIn>();

        var cd = customerGO.AddComponent<CustomerDisplay>();
        cd.neutralIdle = AssetDatabase.LoadAssetAtPath<Sprite>(neutralIdlePath);
        cd.neutralTalk = AssetDatabase.LoadAssetAtPath<Sprite>(neutralTalkPath);
        cd.talk1       = AssetDatabase.LoadAssetAtPath<Sprite>(talk1Path);
        cd.talk2       = AssetDatabase.LoadAssetAtPath<Sprite>(talk2Path);
        cd.happyIdle   = AssetDatabase.LoadAssetAtPath<Sprite>(happyIdlePath);
        cd.surprised   = AssetDatabase.LoadAssetAtPath<Sprite>(surprisedPath);
        cd.gestureIdle = AssetDatabase.LoadAssetAtPath<Sprite>(gestureIdlePath);
        cd.gestureTalk = AssetDatabase.LoadAssetAtPath<Sprite>(gestureTalkPath);

        // Bootstrap 참조 연결
        boot.persistentCanvas = pCanvas;
        boot.customerStage    = stageRT;
        boot.background       = bgRT;
        boot.customerDisplay  = cd;

        // ── SceneCanvas (씬 전용 UI — 대사창) ─────────────────
        var sCanvasGO = new GameObject("SceneCanvas");
        var sCanvas   = sCanvasGO.AddComponent<Canvas>();
        sCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        sCanvas.sortingOrder = 10;
        var sScaler = sCanvasGO.AddComponent<CanvasScaler>();
        sScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sScaler.referenceResolution = new Vector2(1280, 720);
        sScaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        sScaler.matchWidthOrHeight  = 0.5f;
        sCanvasGO.AddComponent<GraphicRaycaster>();

        // DialogueBox — 중앙, 760×200
        var dlgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(dlgBoxPath);
        var dlgGO     = new GameObject("DialogueBox");
        dlgGO.transform.SetParent(sCanvasGO.transform, false);
        var dlgImg    = dlgGO.AddComponent<Image>();
        var dlgGroup  = dlgGO.AddComponent<CanvasGroup>();
        dlgImg.sprite = dlgSprite;
        dlgImg.type   = Image.Type.Sliced;
        dlgImg.color  = Color.white;
        dlgGroup.alpha = 1f;
        if (dlgSprite == null)
        {
            dlgImg.sprite = null;
            dlgImg.color  = new Color(0.96f, 0.93f, 0.84f, 0.97f);
            Debug.LogWarning("[TalkSceneBuilder] DialogueBox.png 없음 — 폴백 색상 사용");
        }
        var dlgRT = dlgGO.GetComponent<RectTransform>();
        dlgRT.anchoredPosition = new Vector2(0, -260);
        dlgRT.sizeDelta        = new Vector2(760, 200);

        // DialogueText
        var textGO = new GameObject("DialogueText");
        textGO.transform.SetParent(dlgGO.transform, false);
        var txt = textGO.AddComponent<Text>();
        txt.text      = "";
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 22;
        txt.color     = new Color(0.20f, 0.12f, 0.06f);
        txt.alignment = TextAnchor.MiddleLeft;
        var txtRT = textGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(22, 38);
        txtRT.offsetMax = new Vector2(-22, -108);

        // ContinueArrow
        var arrowGO = new GameObject("ContinueArrow");
        arrowGO.transform.SetParent(dlgGO.transform, false);
        var arrowTxt = arrowGO.AddComponent<Text>();
        arrowTxt.text      = "▼";
        arrowTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        arrowTxt.fontSize  = 14;
        arrowTxt.color     = new Color(0.42f, 0.30f, 0.16f);
        arrowTxt.alignment = TextAnchor.MiddleCenter;
        var arrowRT = arrowGO.GetComponent<RectTransform>();
        arrowRT.anchorMin        = new Vector2(1f, 0f);
        arrowRT.anchorMax        = new Vector2(1f, 0f);
        arrowRT.pivot            = new Vector2(1f, 0f);
        arrowRT.anchoredPosition = new Vector2(-10f, 10f);
        arrowRT.sizeDelta        = new Vector2(20f, 20f);

        // SceneTransition — 전환 중 씬 로드 이후까지 살아있는 싱글턴
        var transitionGO = new GameObject("SceneTransition");
        var transition   = transitionGO.AddComponent<SceneTransition>();

        // TalkSceneController — Phase별 대사 출력
        var controllerGO = new GameObject("TalkSceneController");
        var controller   = controllerGO.AddComponent<TalkSceneController>();
        controller.customerDisplay  = cd;
        controller.dialogueText     = txt;
        controller.continueArrow    = arrowTxt;
        controller.dialogueBoxGroup = dlgGroup;
        controller.sceneTransition  = transition;

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/TalkScene.unity");
        Debug.Log("[TalkSceneBuilder] TalkScene 생성 완료");
    }
}
