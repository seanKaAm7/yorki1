using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class SceneABuilder
{
    // 배경
    const string bgPath        = "Assets/Sprites/SceneA/BG_Street.png";
    const string dlgBoxPath    = "Assets/Sprites/SceneA/DialogueBox.png";

    // 9-slice border 값 (픽셀 단위) — 1523×513 기준
    const int dlgBorderL = 80, dlgBorderR = 80, dlgBorderT = 100, dlgBorderB = 30;

    // 손님 스프라이트 (전체 세트)
    const string neutralIdlePath   = "Assets/Sprites/SceneA/Customer_Neutral_Idle.png";
    const string neutralTalkPath   = "Assets/Sprites/SceneA/Customer_Neutral_Talk.png";
    const string happyIdlePath     = "Assets/Sprites/SceneA/Customer_Happy_Idle.png";
    const string surprisedPath     = "Assets/Sprites/SceneA/Customer_Surprised.png";
    const string gestureIdlePath   = "Assets/Sprites/SceneA/Customer_Gesture_Idle.png";
    const string gestureTalkPath   = "Assets/Sprites/SceneA/Customer_Gesture_Talk.png";

    [MenuItem("Yorki/Build Scene A")]
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
            neutralIdlePath, neutralTalkPath,
            happyIdlePath,
            surprisedPath,
            gestureIdlePath, gestureTalkPath
        };

        foreach (var p in paths)
        {
            var imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp == null) { Debug.LogWarning("못 찾음: " + p); continue; }
            imp.textureType          = TextureImporterType.Sprite;
            imp.filterMode           = FilterMode.Point;
            imp.textureCompression   = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled        = false;
            imp.alphaIsTransparency  = true;
            imp.SaveAndReimport();
        }
        // DialogueBox — 9-slice border 설정
        var dlgImp = AssetImporter.GetAtPath(dlgBoxPath) as TextureImporter;
        if (dlgImp != null)
        {
            dlgImp.textureType         = TextureImporterType.Sprite;
            dlgImp.spriteImportMode    = SpriteImportMode.Single;  // Multiple 모드 충돌 방지
            dlgImp.filterMode          = FilterMode.Bilinear;
            dlgImp.textureCompression  = TextureImporterCompression.Uncompressed;
            dlgImp.mipmapEnabled       = false;
            dlgImp.alphaIsTransparency = true;
            dlgImp.spriteBorder        = new Vector4(dlgBorderL, dlgBorderB, dlgBorderR, dlgBorderT);
            dlgImp.SaveAndReimport();
        }

        Debug.Log("[SceneABuilder] 스프라이트 설정 완료");
    }

    static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 카메라
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        cam.orthographic    = true;
        camGO.tag           = "MainCamera";

        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // 배경 — BG는 2780×1504 (비율 1.848). 높이 720 기준 폭 역산: 720×1.848=1330
        // preserveAspect=true + (1330,720) 컨테이너 → 비율 정확히 일치, 레터박스/찌그러짐 없음
        var bgGO  = MakeImage("Background", canvasGO.transform,
            AssetDatabase.LoadAssetAtPath<Sprite>(bgPath),
            new Vector2(-320, 0), new Vector2(1330, 720));
        var bgImg = bgGO.GetComponent<Image>();
        bgImg.preserveAspect = true;

        // 오른쪽 패널 (그림 영역 — 현재 빈 공간)
        var rightGO = MakeImage("RightPanel", canvasGO.transform, null,
            new Vector2(320, 0), new Vector2(640, 720));
        rightGO.GetComponent<Image>().color = new Color(0.15f, 0.12f, 0.1f);

        // 손님 캐릭터 — 패널 중앙, 초안.png 비율 기준
        var customerGO  = MakeImage("Customer", canvasGO.transform,
            AssetDatabase.LoadAssetAtPath<Sprite>(neutralIdlePath),
            new Vector2(-320, -50), new Vector2(320, 320));
        var customerImg = customerGO.GetComponent<Image>();
        customerImg.preserveAspect = true;

        // FadeIn 연출
        customerGO.AddComponent<FadeIn>();

        // CustomerDisplay — 스프라이트 슬롯 자동 할당
        var cd           = customerGO.AddComponent<CustomerDisplay>();
        cd.neutralIdle   = AssetDatabase.LoadAssetAtPath<Sprite>(neutralIdlePath);
        cd.neutralTalk   = AssetDatabase.LoadAssetAtPath<Sprite>(neutralTalkPath);
        cd.happyIdle     = AssetDatabase.LoadAssetAtPath<Sprite>(happyIdlePath);
        cd.surprised     = AssetDatabase.LoadAssetAtPath<Sprite>(surprisedPath);
        cd.gestureIdle   = AssetDatabase.LoadAssetAtPath<Sprite>(gestureIdlePath);
        cd.gestureTalk   = AssetDatabase.LoadAssetAtPath<Sprite>(gestureTalkPath);

        // 대사창
        var dlgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(dlgBoxPath);
        var dlgGO     = MakeImage("DialogueBox", canvasGO.transform, dlgSprite,
            new Vector2(-320, -268), new Vector2(600, 200));
        var dlgImg    = dlgGO.GetComponent<Image>();
        dlgImg.type   = Image.Type.Sliced;
        dlgImg.color  = Color.white;
        // PNG 없을 때 폴백 — 단순 색상으로 표시
        if (dlgSprite == null)
        {
            dlgImg.sprite = null;
            dlgImg.color  = new Color(0.96f, 0.93f, 0.84f, 0.97f);
            Debug.LogWarning("[SceneABuilder] DialogueBox.png 없음 — 폴백 색상 사용");
        }

        // 대사 텍스트
        var textGO    = new GameObject("DialogueText");
        textGO.transform.SetParent(dlgGO.transform, false);
        var txt       = textGO.AddComponent<Text>();
        txt.text      = "";
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 22;
        txt.color     = new Color(0.20f, 0.12f, 0.06f);
        txt.alignment = TextAnchor.MiddleLeft;
        var txtRT     = textGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        // T=100(triangle+border), B=30(border) 기준으로 텍스트 영역 설정
        txtRT.offsetMin = new Vector2(22, 38);
        txtRT.offsetMax = new Vector2(-22, -108);

        // ▼ 계속 표시 — 우하단 고정 (SceneADialogue에서 깜빡임 제어)
        var arrowGO   = new GameObject("ContinueArrow");
        arrowGO.transform.SetParent(dlgGO.transform, false);
        var arrowTxt  = arrowGO.AddComponent<Text>();
        arrowTxt.text      = "▼";
        arrowTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        arrowTxt.fontSize  = 14;
        arrowTxt.color     = new Color(0.42f, 0.30f, 0.16f); // #6B4D2A
        arrowTxt.alignment = TextAnchor.MiddleCenter;
        var arrowRT   = arrowGO.GetComponent<RectTransform>();
        arrowRT.anchorMin        = new Vector2(1f, 0f);
        arrowRT.anchorMax        = new Vector2(1f, 0f);
        arrowRT.pivot            = new Vector2(1f, 0f);
        arrowRT.anchoredPosition = new Vector2(-10f, 10f);
        arrowRT.sizeDelta        = new Vector2(20f, 20f);

        // SceneADialogue — 대사 진행 컨트롤러
        var dialogueCtrl          = customerGO.AddComponent<SceneADialogue>();
        dialogueCtrl.customerDisplay = cd;
        dialogueCtrl.dialogueText    = txt;
        dialogueCtrl.continueArrow   = arrowTxt;

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SceneA.unity");
        Debug.Log("[SceneABuilder] SceneA 생성 완료");
    }

    static GameObject MakeImage(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        var rt  = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        return go;
    }
}
