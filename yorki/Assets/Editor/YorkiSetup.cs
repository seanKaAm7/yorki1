using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class YorkiSetup
{
    static readonly string spriteRoot = "Assets/Sprites/Yorki";
    static readonly string animFolder = "quiet_natural_walk_calm_relaxed_pace_slightly_tire-221430fb";
    static readonly string[] dirs = { "south", "south-east", "east", "north-east", "north", "north-west", "west", "south-west" };

    [MenuItem("Yorki/Setup Yorki Character")]
    public static void Run()
    {
        SetupSprites();
        AssetDatabase.Refresh();
        BuildAnimations();
        BuildTestScene();
        Debug.Log("[YorkiSetup] 완료!");
    }

    static void SetupSprites()
    {
        var paths = new System.Collections.Generic.List<string>();
        foreach (var d in dirs)
            paths.Add($"{spriteRoot}/rotations/{d}.png");
        foreach (var d in dirs)
            for (int i = 0; i < 6; i++)
                paths.Add($"{spriteRoot}/animations/{animFolder}/{d}/frame_{i:D3}.png");

        foreach (var p in paths)
        {
            var imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp == null) { Debug.LogWarning($"못 찾음: {p}"); continue; }
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 68f;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.SaveAndReimport();
        }
        Debug.Log("[YorkiSetup] 스프라이트 설정 완료");
    }

    static void BuildAnimations()
    {
        string clipDir = "Assets/Animations/Yorki";
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(clipDir))
            AssetDatabase.CreateFolder("Assets/Animations", "Yorki");

        // idle 클립
        foreach (var d in dirs)
        {
            string spritePath = $"{spriteRoot}/rotations/{d}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null) continue;

            var clip = new AnimationClip();
            clip.frameRate = 6;
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var kf = new ObjectReferenceKeyframe[2];
            kf[0] = new ObjectReferenceKeyframe { time = 0f,        value = sprite };
            kf[1] = new ObjectReferenceKeyframe { time = 1f / 6f,   value = sprite };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, kf);
            AssetDatabase.CreateAsset(clip, $"{clipDir}/idle_{d}.anim");
        }

        // walk 클립
        foreach (var d in dirs)
        {
            var clip = new AnimationClip();
            clip.frameRate = 6;
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var kf = new ObjectReferenceKeyframe[6];
            for (int i = 0; i < 6; i++)
            {
                string fp = $"{spriteRoot}/animations/{animFolder}/{d}/frame_{i:D3}.png";
                kf[i] = new ObjectReferenceKeyframe { time = i / 6f, value = AssetDatabase.LoadAssetAtPath<Sprite>(fp) };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, kf);
            var s = AnimationUtility.GetAnimationClipSettings(clip);
            s.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, s);
            AssetDatabase.CreateAsset(clip, $"{clipDir}/walk_{d}.anim");
        }

        AssetDatabase.SaveAssets();

        // Animator Controller
        string ctrlPath = "Assets/Animations/Yorki/YorkiController.controller";
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        ctrl.AddParameter("isMoving", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("dirIndex", AnimatorControllerParameterType.Int);

        var sm = ctrl.layers[0].stateMachine;

        // 모든 방향 상태 먼저 생성
        for (int i = 0; i < dirs.Length; i++)
        {
            string d = dirs[i];
            var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{clipDir}/idle_{d}.anim");
            var walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{clipDir}/walk_{d}.anim");

            var idle = sm.AddState($"idle_{d}", new Vector3(300, i * 100, 0));
            idle.motion = idleClip;

            var walk = sm.AddState($"walk_{d}", new Vector3(600, i * 100, 0));
            walk.motion = walkClip;
        }

        // AnyState → 각 상태 전환 (어느 상태에서든 방향/이동 변경 시 즉시 전환)
        for (int i = 0; i < dirs.Length; i++)
        {
            var idleState = sm.states[i * 2].state;
            var walkState = sm.states[i * 2 + 1].state;

            // AnyState → walk_{d}
            var tw = sm.AddAnyStateTransition(walkState);
            tw.hasExitTime = false;
            tw.duration = 0;
            tw.canTransitionToSelf = false;
            tw.AddCondition(AnimatorConditionMode.If, 0, "isMoving");
            tw.AddCondition(AnimatorConditionMode.Equals, i, "dirIndex");

            // AnyState → idle_{d}
            var ti = sm.AddAnyStateTransition(idleState);
            ti.hasExitTime = false;
            ti.duration = 0;
            ti.canTransitionToSelf = false;
            ti.AddCondition(AnimatorConditionMode.IfNot, 0, "isMoving");
            ti.AddCondition(AnimatorConditionMode.Equals, i, "dirIndex");
        }

        sm.defaultState = sm.states[0].state;
        AssetDatabase.SaveAssets();
        Debug.Log("[YorkiSetup] 애니메이션 + Controller 완료");
    }

    static void BuildTestScene()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
            UnityEditor.SceneManagement.NewSceneMode.Single);

        var ground = new GameObject("Ground");
        var sr = ground.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.3f, 0.55f, 0.3f);
        ground.transform.localScale = new Vector3(30f, 20f, 1f);

        var yorki = new GameObject("Yorki");
        yorki.transform.position = Vector3.zero;
        var yorkiSR = yorki.AddComponent<SpriteRenderer>();
        yorkiSR.sortingOrder = 1;

        var animator = yorki.AddComponent<Animator>();
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Yorki/YorkiController.controller");
        animator.runtimeAnimatorController = ctrl;

        yorki.AddComponent<YorkiMovement>();

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/SceneB_Test.unity");
        Debug.Log("[YorkiSetup] SceneB_Test 씬 생성 완료");
    }
}
