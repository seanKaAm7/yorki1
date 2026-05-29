using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

// 대화 씬 빌더 — 풀화면 배경 + 손님(중앙) + 대사창.
// 이 씬에 영속 객체(PersistentBootstrap + CustomerStage + Customer + Background)도 함께 만들어둔다.
// 게임 시작 시 TalkScene이 먼저 로드되면, PersistentBootstrap.Awake에서 DontDestroyOnLoad 처리.
public class TalkSceneBuilder
{
    // 배경
    const string bgPath        = "Assets/Sprites/SceneA/MapBackgroundDraft2.png";

    // TalkScene 대화창 색상
    static readonly Color DialogueBoxColor = new Color(0.055f, 0.066f, 0.070f, 0.78f);
    static readonly Color DialogueBoxOutlineColor = new Color(0.16f, 0.18f, 0.19f, 0.70f);
    static readonly Color DialogueTextColor = new Color(0.95f, 0.95f, 0.90f, 1f);
    static readonly Color ContinueArrowColor = new Color(0.94f, 0.90f, 0.70f, 1f);
    static readonly Color SpeakerNameColor = new Color(0.94f, 0.90f, 0.70f, 1f);
    static readonly Color HudPanelColor = new Color(0.055f, 0.050f, 0.046f, 0.82f);
    static readonly Color HudOutlineColor = new Color(0.38f, 0.28f, 0.20f, 0.72f);
    static readonly Color HudTextColor = new Color(0.92f, 0.90f, 0.84f, 1f);
    static readonly Color HudMutedTextColor = new Color(0.76f, 0.72f, 0.64f, 1f);
    static readonly Color HudEnergyColor = new Color(0.47f, 0.72f, 0.28f, 1f);
    static readonly Color HudReputationColor = new Color(0.88f, 0.63f, 0.10f, 1f);
    static readonly Color HudMaterialsColor = new Color(0.70f, 0.47f, 0.24f, 1f);

    // 손님 컷 (Neutral 입 단계 4종 + 그 외 감정)
    const string neutralIdlePath = "Assets/Sprites/SceneA/Customer_Neutral_Idle.png";
    const string neutralTalkPath = "Assets/Sprites/SceneA/Customer_Neutral_Talk.png";
    const string talk1Path       = "Assets/Sprites/SceneA/Customer_Talk1.png";
    const string talk2Path       = "Assets/Sprites/SceneA/Customer_Talk2.png";
    const string happyIdlePath   = "Assets/Sprites/SceneA/Customer_Happy_Idle.png";
    const string surprisedPath   = "Assets/Sprites/SceneA/Customer_Surprised.png";
    const string gestureIdlePath = "Assets/Sprites/SceneA/Customer_Gesture_Idle.png";
    const string gestureTalkPath = "Assets/Sprites/SceneA/Customer_Gesture_Talk.png";

    // 기본 에피소드 큐 (빌더 재실행 시 TalkSceneController 참조 자동 복원용)
    const string episode01Path = "Assets/Data/CustomerEpisodes/CustomerEpisode_01_Goro.asset";
    const string episode02Path = "Assets/Data/CustomerEpisodes/CustomerEpisode_02_Hailey.asset";
    const string episode03Path = "Assets/Data/CustomerEpisodes/CustomerEpisode_03_Winter.asset";
    const string defaultEpisodePath = episode01Path;

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
            bgPath,
            neutralIdlePath, neutralTalkPath, talk1Path, talk2Path,
            happyIdlePath, surprisedPath, gestureIdlePath, gestureTalkPath
        };
        foreach (var p in paths)
        {
            YorkiEditorAssets.ConfigureSprite(p, "[TalkSceneBuilder] 못 찾음: ");
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
        customerRT.anchoredPosition = new Vector2(-9.320013f, -29.144989f);
        customerRT.sizeDelta        = new Vector2(652.1983f, 778.29f);

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
        cd.mouthFrameInterval = 3;
        cd.useExtraNeutralTalkFrames = false;

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

        Font uiFont = YorkiEditorAssets.LoadUIFont();
        CreateHUD(sCanvasGO.transform, uiFont);

        // DialogueBox — 어두운 반투명 박스
        var dlgGO     = new GameObject("DialogueBox");
        dlgGO.transform.SetParent(sCanvasGO.transform, false);
        var dlgImg    = dlgGO.AddComponent<Image>();
        var dlgOutline = dlgGO.AddComponent<Outline>();
        var dlgGroup  = dlgGO.AddComponent<CanvasGroup>();
        dlgImg.sprite = null;
        dlgImg.type   = Image.Type.Simple;
        dlgImg.color  = DialogueBoxColor;
        dlgImg.raycastTarget = false;
        dlgOutline.effectColor = DialogueBoxOutlineColor;
        dlgOutline.effectDistance = new Vector2(2f, -2f);
        dlgOutline.useGraphicAlpha = true;
        dlgGroup.alpha = 1f;
        var dlgRT = dlgGO.GetComponent<RectTransform>();
        dlgRT.anchoredPosition = new Vector2(0f, -250f);
        dlgRT.sizeDelta        = new Vector2(820f, 160f);

        // SpeakerNameText
        var nameGO = new GameObject("SpeakerNameText");
        nameGO.transform.SetParent(dlgGO.transform, false);
        var nameTxt = nameGO.AddComponent<Text>();
        nameTxt.text      = "";
        nameTxt.font      = uiFont;
        nameTxt.fontSize  = 18;
        nameTxt.color     = SpeakerNameColor;
        nameTxt.alignment = TextAnchor.UpperLeft;
        var nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin        = new Vector2(0f, 1f);
        nameRT.anchorMax        = new Vector2(1f, 1f);
        nameRT.pivot            = new Vector2(0f, 1f);
        nameRT.anchoredPosition = new Vector2(36f, -18f);
        nameRT.sizeDelta        = new Vector2(-72f, 24f);

        // DialogueText
        var textGO = new GameObject("DialogueText");
        textGO.transform.SetParent(dlgGO.transform, false);
        var txt = textGO.AddComponent<Text>();
        txt.text      = "";
        txt.font      = uiFont;
        txt.fontSize  = 22;
        txt.color     = DialogueTextColor;
        txt.alignment = TextAnchor.UpperLeft;
        var txtRT = textGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(36f, 28f);
        txtRT.offsetMax = new Vector2(-36f, -46f);

        // ContinueArrow
        var arrowGO = new GameObject("ContinueArrow");
        arrowGO.transform.SetParent(dlgGO.transform, false);
        var arrowTxt = arrowGO.AddComponent<Text>();
        arrowTxt.text      = "▼";
        arrowTxt.font      = uiFont;
        arrowTxt.fontSize  = 14;
        arrowTxt.color     = ContinueArrowColor;
        arrowTxt.alignment = TextAnchor.MiddleCenter;
        var arrowRT = arrowGO.GetComponent<RectTransform>();
        arrowRT.anchorMin        = new Vector2(1f, 0f);
        arrowRT.anchorMax        = new Vector2(1f, 0f);
        arrowRT.pivot            = new Vector2(1f, 0f);
        arrowRT.anchoredPosition = new Vector2(-18f, 16f);
        arrowRT.sizeDelta        = new Vector2(20f, 20f);

        // SceneTransition — 전환 중 씬 로드 이후까지 살아있는 싱글턴
        var transitionGO = new GameObject("SceneTransition");
        var transition   = transitionGO.AddComponent<SceneTransition>();

        // TalkSceneController — Phase별 대사 출력
        var controllerGO = new GameObject("TalkSceneController");
        var controller   = controllerGO.AddComponent<TalkSceneController>();
        controller.customerDisplay  = cd;
        controller.dialogueText     = txt;
        controller.speakerNameText  = nameTxt;
        controller.continueArrow    = arrowTxt;
        controller.dialogueBoxGroup = dlgGroup;
        controller.sceneTransition  = transition;
        controller.introSpeakerName = "요르키";
        controller.introMonologueLines = new DialogueLineData[]
        {
            new DialogueLineData
            {
                speakerName = "요르키",
                emotion = "thinking",
                text = "오늘도 이 거리 한복판에서 시작이다.",
                shake = false
            },
            new DialogueLineData
            {
                speakerName = "요르키",
                emotion = "thinking",
                text = "첫 손님이 오기 전에, 손부터 조금 풀어두자.",
                shake = false
            }
        };

        var defaultEpisode = AssetDatabase.LoadAssetAtPath<CustomerEpisodeData>(defaultEpisodePath);
        if (defaultEpisode != null)
            controller.currentEpisode = defaultEpisode;
        else
            Debug.LogWarning($"[TalkSceneBuilder] 기본 에피소드를 찾지 못함: {defaultEpisodePath}");

        var haileyEpisode = AssetDatabase.LoadAssetAtPath<CustomerEpisodeData>(episode02Path);
        var winterEpisode = AssetDatabase.LoadAssetAtPath<CustomerEpisodeData>(episode03Path);
        controller.dayEpisodeQueue = new CustomerEpisodeData[]
        {
            defaultEpisode,
            haileyEpisode,
            winterEpisode
        };

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/TalkScene.unity");
        Debug.Log("[TalkSceneBuilder] TalkScene 생성 완료");
    }

    static TalkSceneHUDController CreateHUD(Transform parent, Font uiFont)
    {
        var hudGO = new GameObject("TalkSceneHUD", typeof(RectTransform));
        hudGO.transform.SetParent(parent, false);
        var hudRT = hudGO.GetComponent<RectTransform>();
        hudRT.anchorMin = Vector2.zero;
        hudRT.anchorMax = Vector2.one;
        hudRT.offsetMin = Vector2.zero;
        hudRT.offsetMax = Vector2.zero;

        var hud = hudGO.AddComponent<TalkSceneHUDController>();

        var header = CreatePanel(hudGO.transform, "HUD_LeftHeader", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -8f), new Vector2(250f, 98f));
        CreateText(header.transform, "CafeIconText", uiFont, "CAFE", 13, HudMutedTextColor, TextAnchor.MiddleCenter,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -14f), new Vector2(48f, 28f));
        hud.placeText = CreateText(header.transform, "PlaceText", uiFont, "Zum Goldenen Hahn", 17, HudTextColor, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(70f, -12f), new Vector2(-82f, 24f));
        hud.dayText = CreateText(header.transform, "DayText", uiFont, "Day 7", 20, HudTextColor, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(72f, -43f), new Vector2(80f, 28f));
        hud.clockText = CreateText(header.transform, "ClockText", uiFont, "10:30", 20, HudTextColor, TextAnchor.UpperRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-14f, -43f), new Vector2(82f, 28f));
        hud.seasonText = CreateText(header.transform, "SeasonText", uiFont, "섬괴옥", 16, HudTextColor, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(72f, -72f), new Vector2(76f, 22f));
        hud.weekdayText = CreateText(header.transform, "WeekdayText", uiFont, "월요일", 16, HudTextColor, TextAnchor.UpperRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -72f), new Vector2(82f, 22f));

        var stats = CreatePanel(hudGO.transform, "HUD_LeftStats", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -114f), new Vector2(250f, 238f));
        Image unusedFill;
        Text unusedValue;
        CreateStatRow(stats.transform, uiFont, hud, "수입", 18f, Color.clear, false, TalkSceneHUDStatKind.Energy, out hud.incomeValueText, out unusedFill);
        CreateStatRow(stats.transform, uiFont, hud, "시간", 60f, Color.clear, false, TalkSceneHUDStatKind.Energy, out hud.timeValueText, out unusedFill);
        CreateStatRow(stats.transform, uiFont, hud, "에너지", 102f, HudEnergyColor, true, TalkSceneHUDStatKind.Energy, out unusedValue, out hud.energyFill);
        CreateStatRow(stats.transform, uiFont, hud, "평판", 144f, HudReputationColor, true, TalkSceneHUDStatKind.Reputation, out unusedValue, out hud.reputationFill);
        CreateStatRow(stats.transform, uiFont, hud, "재료", 186f, HudMaterialsColor, true, TalkSceneHUDStatKind.Materials, out unusedValue, out hud.materialsFill);

        var settings = CreatePanel(hudGO.transform, "HUD_SettingsButton", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-62f, -8f), new Vector2(48f, 48f));
        CreateText(settings.transform, "SettingsText", uiFont, "설정", 14, HudTextColor, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var records = CreatePanel(hudGO.transform, "HUD_RecordsButton", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-8f, -8f), new Vector2(48f, 48f));
        CreateText(records.transform, "RecordsText", uiFont, "기록", 14, HudTextColor, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        var goal = CreatePanel(hudGO.transform, "HUD_GoalPanel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-8f, -96f), new Vector2(285f, 112f));
        hud.goalTitleText = CreateText(goal.transform, "GoalTitleText", uiFont, "다음 목표", 16, HudTextColor, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(-36f, 24f));
        hud.goalBodyText = CreateText(goal.transform, "GoalBodyText", uiFont, "오늘 손님 3명을 맞이해보세요!", 16, HudTextColor, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(18f, -43f), new Vector2(-36f, 42f));
        hud.goalProgressText = CreateText(goal.transform, "GoalProgressText", uiFont, "(0 / 3)", 16, HudTextColor, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(18f, -82f), new Vector2(-36f, 24f));

        var reservation = CreatePanel(hudGO.transform, "HUD_ReservationPanel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-8f, -218f), new Vector2(285f, 84f));
        CreateText(reservation.transform, "ReservationTitleText", uiFont, "오늘의 예약", 16, HudTextColor, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(-36f, 24f));
        hud.reservationBodyText = CreateText(reservation.transform, "ReservationBodyText", uiFont, "없음", 16, HudTextColor, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(18f, -48f), new Vector2(-36f, 24f));

        CreateStatTooltip(hudGO.transform, uiFont, hud);

        return hud;
    }

    static Image CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = HudPanelColor;
        image.raycastTarget = false;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = HudOutlineColor;
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = size;
        return image;
    }

    static Text CreateText(Transform parent, string name, Font font, string text, int fontSize, Color color, TextAnchor alignment,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var uiText = go.AddComponent<Text>();
        uiText.text = text;
        uiText.font = font;
        uiText.fontSize = fontSize;
        uiText.color = color;
        uiText.alignment = alignment;
        uiText.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;
        return uiText;
    }

    static void CreateStatRow(Transform parent, Font font, TalkSceneHUDController hud, string label, float topY, Color fillColor, bool hasBar, TalkSceneHUDStatKind statKind, out Text valueText, out Image fillImage)
    {
        CreateText(parent, $"Label_{label}", font, label, 16, HudTextColor, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -topY), new Vector2(80f, 24f));
        fillImage = null;

        if (!hasBar)
        {
            valueText = CreateText(parent, $"Value_{label}", font, "", 16, HudTextColor, TextAnchor.UpperRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -topY), new Vector2(100f, 24f));
            return;
        }

        valueText = null;

        var barBack = new GameObject($"Bar_{label}", typeof(RectTransform));
        barBack.transform.SetParent(parent, false);
        var backImage = barBack.AddComponent<Image>();
        backImage.color = new Color(0.13f, 0.12f, 0.11f, 0.92f);
        backImage.raycastTarget = true;
        var backRT = barBack.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0f, 1f);
        backRT.anchorMax = new Vector2(0f, 1f);
        backRT.pivot = new Vector2(0f, 1f);
        backRT.anchoredPosition = new Vector2(108f, -topY - 4f);
        backRT.sizeDelta = new Vector2(124f, 20f);

        var tooltipTarget = barBack.AddComponent<TalkSceneHUDTooltipTarget>();
        tooltipTarget.hud = hud;
        tooltipTarget.statKind = statKind;

        var fillGO = new GameObject($"Fill_{label}", typeof(RectTransform));
        fillGO.transform.SetParent(barBack.transform, false);
        fillImage = fillGO.AddComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 0.5f;
        fillImage.raycastTarget = false;
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        barBack.transform.SetSiblingIndex(1);
    }

    static void CreateStatTooltip(Transform parent, Font font, TalkSceneHUDController hud)
    {
        var tooltip = CreatePanel(parent, "HUD_StatTooltip", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(256f, -196f), new Vector2(140f, 34f));
        tooltip.color = new Color(0.055f, 0.050f, 0.046f, 0.94f);
        hud.statTooltipRoot = tooltip.GetComponent<RectTransform>();
        hud.statTooltipText = CreateText(tooltip.transform, "TooltipText", font, "", 15, HudTextColor, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        tooltip.gameObject.SetActive(false);
    }
}
