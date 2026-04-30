using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    public static readonly Vector2 TalkStagePosition = Vector2.zero;
    public static readonly Vector2 DrawingStagePosition = new Vector2(-320f, 0f);

    [Header("Settings")]
    public float duration = 0.7f;
    public float loadAt = 0.5f;
    public float fadeDuration = 0.3f;

    public bool IsTransitioning { get; private set; }

    public static SceneTransition EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        SceneTransition existing = Object.FindAnyObjectByType<SceneTransition>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject("SceneTransition");
        return go.AddComponent<SceneTransition>();
    }

    public static CanvasGroup EnsureCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
            group = target.AddComponent<CanvasGroup>();
        return group;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (!IsTransitioning)
            PlaceStageForActiveScene();
    }

    public void TalkSceneToSceneA()
    {
        if (IsTransitioning) return;
        StartCoroutine(TransitionRoutine("SceneA", DrawingStagePosition, true, TalkScenePhase.PreDraw));
    }

    public void SceneAToTalkScene(TalkScenePhase nextPhase)
    {
        if (IsTransitioning) return;

        GameManager.currentTalkPhase = nextPhase;
        StartCoroutine(TransitionRoutine("TalkScene", TalkStagePosition, false, nextPhase));
    }

    public void PlaceStageForTalkScene()
    {
        RectTransform stage = FindCustomerStage();
        if (stage != null)
            stage.anchoredPosition = TalkStagePosition;
    }

    public void PlaceStageForSceneA()
    {
        RectTransform stage = FindCustomerStage();
        if (stage != null)
            stage.anchoredPosition = DrawingStagePosition;
    }

    void PlaceStageForActiveScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "SceneA")
            PlaceStageForSceneA();
        else if (sceneName == "TalkScene")
            PlaceStageForTalkScene();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsTransitioning)
            PlaceStageForActiveScene();
    }

    IEnumerator TransitionRoutine(string targetScene, Vector2 targetPosition, bool toSceneA, TalkScenePhase nextPhase)
    {
        IsTransitioning = true;

        RectTransform stage = FindCustomerStage();
        Vector2 startPosition = stage != null ? stage.anchoredPosition : (toSceneA ? TalkStagePosition : DrawingStagePosition);

        CanvasGroup outgoingGroup = toSceneA ? FindCanvasGroup("DialogueBox") : FindCanvasGroup("RightPanel");
        CanvasGroup incomingGroup = null;
        bool loaded = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseInOutQuad(t);

            if (stage == null)
                stage = FindCustomerStage();

            if (stage != null)
                stage.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, eased);

            if (outgoingGroup != null)
                outgoingGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(elapsed / fadeDuration));

            if (!loaded && elapsed >= loadAt)
            {
                loaded = true;
                GameManager.currentTalkPhase = nextPhase;
                SceneManager.LoadScene(targetScene);
                yield return null;

                incomingGroup = toSceneA ? FindCanvasGroup("RightPanel") : FindCanvasGroup("DialogueBox");
                if (incomingGroup != null)
                {
                    incomingGroup.alpha = 0f;
                    StartCoroutine(FadeCanvasGroup(incomingGroup, 0f, 1f, fadeDuration));
                }
            }

            yield return null;
        }

        if (stage == null)
            stage = FindCustomerStage();

        if (stage != null)
            stage.anchoredPosition = targetPosition;

        IsTransitioning = false;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float time)
    {
        if (group == null) yield break;

        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / time));
            yield return null;
        }

        group.alpha = to;
    }

    RectTransform FindCustomerStage()
    {
        if (PersistentBootstrap.Instance != null && PersistentBootstrap.Instance.customerStage != null)
            return PersistentBootstrap.Instance.customerStage;

        GameObject go = GameObject.Find("CustomerStage");
        return go != null ? go.GetComponent<RectTransform>() : null;
    }

    CanvasGroup FindCanvasGroup(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        return go != null ? EnsureCanvasGroup(go) : null;
    }

    static float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
    }
}
