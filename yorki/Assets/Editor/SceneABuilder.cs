using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

// 캔버스 모드 씬 빌더.
// 손님·배경·대사창은 PersistentBootstrap(영속 객체)가 보유하므로 여기선 만들지 않음.
// 작업대(RightPanel) + 카메라/캔버스만 생성.
public class SceneABuilder
{
    [MenuItem("Yorki/Build Scene A")]
    public static void Build()
    {
        BuildScene();
    }

    static void BuildScene()
    {
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

        // RightPanel — 작업대 자리 (드로잉 UI는 추후)
        var rightGO = new GameObject("RightPanel");
        rightGO.transform.SetParent(canvasGO.transform, false);
        var rightImg = rightGO.AddComponent<Image>();
        var rightGroup = rightGO.AddComponent<CanvasGroup>();
        rightImg.color = new Color(0.15f, 0.12f, 0.10f);
        rightGroup.alpha = 1f;
        var rightRT = rightGO.GetComponent<RectTransform>();
        rightRT.anchoredPosition = new Vector2(320, 0);
        rightRT.sizeDelta        = new Vector2(640, 720);

        // SceneTransition — TalkScene에서 넘어온 싱글턴이 없을 때 직접 실행용 폴백
        var transitionGO = new GameObject("SceneTransition");
        transitionGO.AddComponent<SceneTransition>();
        transitionGO.AddComponent<SceneATestTransitionInput>();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SceneA.unity");
        Debug.Log("[SceneABuilder] SceneA 생성 완료 — 작업대만 (손님/배경/대사창은 PersistentBootstrap 영속 객체)");
    }
}
