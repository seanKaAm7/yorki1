using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    public static readonly Vector2 StagePosition = Vector2.zero;
    public static readonly Vector2 TalkCustomerPosition = new Vector2(-9.320013f, -107.3f);
    public static readonly Vector2 DrawingCustomerPosition = new Vector2(-329.320013f, -107.3f);

    [Header("Settings")]
    public float duration = 0.7f;
    public float loadAt = 0.5f;
    public float fadeDuration = 0.3f;

    public bool IsTransitioning { get; private set; }

    public static SceneTransition EnsureInstance() // 싱글톤 인스턴스 보장. 이미 존재하면 반환, 그렇지 않으면 씬에서 찾아보고 없으면 새로 생성.
    {
        if (Instance != null)
            return Instance;

        SceneTransition existing = Object.FindAnyObjectByType<SceneTransition>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject(YorkiObjectNames.SceneTransition);
        return go.AddComponent<SceneTransition>();
    }

    public static CanvasGroup EnsureCanvasGroup(GameObject target) // CanvasGroup 컴포넌트 보장. 이미 존재하면 반환, 그렇지 않으면 새로 생성.
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
            group = target.AddComponent<CanvasGroup>();
        return group;
    }

    void Awake() // 싱글톤 패턴 구현. 이미 인스턴스가 존재하면 자신을 파괴하고, 그렇지 않으면 인스턴스로 설정하고 씬 전환 시에도 파괴되지 않도록 함.
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() // 씬이 로드될 때마다 고객 스테이지 위치를 조정하기 위해 SceneManager.sceneLoaded 이벤트에 핸들러 등록.
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
        ResetCustomerForDrawing();
        StartCoroutine(TransitionRoutine(YorkiSceneNames.Drawing, DrawingCustomerPosition, true, TalkScenePhase.PreDraw));
    }

    public void SceneAToTalkScene(TalkScenePhase nextPhase) // SceneA에서 TalkScene으로 전환. 다음 대화 단계(nextPhase)를 인자로 받아서 씬이 로드될 때 GameManager.currentTalkPhase에 설정하도록 함.
    {
        if (IsTransitioning) return;

        GameManager.currentTalkPhase = nextPhase;
        StartCoroutine(TransitionRoutine(YorkiSceneNames.Talk, TalkCustomerPosition, false, nextPhase));
    }

    public void PlaceStageForTalkScene()
    {
        PlaceStageAndCustomer(TalkCustomerPosition);
    }

    public void PlaceStageForSceneA()
    {
        PlaceStageAndCustomer(DrawingCustomerPosition);
        ResetCustomerForDrawing();
    }

    void PlaceStageForActiveScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == YorkiSceneNames.Drawing)
            PlaceStageForSceneA();
        else if (sceneName == YorkiSceneNames.Talk)
            PlaceStageForTalkScene();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsTransitioning)
            PlaceStageForActiveScene();
    }

    IEnumerator TransitionRoutine(string targetScene, Vector2 targetCustomerPosition, bool toSceneA, TalkScenePhase nextPhase) // 씬 전환 코루틴. 배경 stage는 화면에 고정하고, 손님 RectTransform만 이동한다. toSceneA가 true면 TalkScene에서 SceneA로, false면 SceneA에서 TalkScene으로 전환. nextPhase는 씬이 로드된 후 GameManager.currentTalkPhase에 설정할 값.
    {
        IsTransitioning = true;

        RectTransform stage = FindCustomerStage();
        RectTransform customer = FindCustomerRect();
        Vector2 startCustomerPosition = customer != null ? customer.anchoredPosition : (toSceneA ? TalkCustomerPosition : DrawingCustomerPosition);
        if (stage != null)
            stage.anchoredPosition = StagePosition;

        CanvasGroup outgoingGroup = toSceneA ? FindCanvasGroup(YorkiObjectNames.DialogueBox) : FindCanvasGroup(YorkiObjectNames.DrawingPanel);
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
                stage.anchoredPosition = StagePosition;

            if (customer == null)
                customer = FindCustomerRect();

            if (customer != null)
                customer.anchoredPosition = Vector2.LerpUnclamped(startCustomerPosition, targetCustomerPosition, eased);

            if (outgoingGroup != null)
                outgoingGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(elapsed / fadeDuration));

            if (!loaded && elapsed >= loadAt)
            {
                loaded = true;
                GameManager.currentTalkPhase = nextPhase;
                SceneManager.LoadScene(targetScene);
                yield return null;

                incomingGroup = toSceneA ? FindCanvasGroup(YorkiObjectNames.DrawingPanel) : FindCanvasGroup(YorkiObjectNames.DialogueBox);
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
            stage.anchoredPosition = StagePosition;

        if (customer == null)
            customer = FindCustomerRect();

        if (customer != null)
            customer.anchoredPosition = targetCustomerPosition;

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

        GameObject go = GameObject.Find(YorkiObjectNames.CustomerStage);
        return go != null ? go.GetComponent<RectTransform>() : null;
    }

    RectTransform FindCustomerRect()
    {
        CustomerDisplay customer = null;
        if (PersistentBootstrap.Instance != null)
            customer = PersistentBootstrap.Instance.customerDisplay;
        if (customer == null)
            customer = Object.FindAnyObjectByType<CustomerDisplay>();

        return customer != null ? customer.GetComponent<RectTransform>() : null;
    }

    void PlaceStageAndCustomer(Vector2 customerPosition)
    {
        RectTransform stage = FindCustomerStage();
        if (stage != null)
            stage.anchoredPosition = StagePosition;

        RectTransform customer = FindCustomerRect();
        if (customer != null)
            customer.anchoredPosition = customerPosition;
    }

    void ResetCustomerForDrawing()
    {
        CustomerDisplay customer = null;
        if (PersistentBootstrap.Instance != null)
            customer = PersistentBootstrap.Instance.customerDisplay;
        if (customer == null)
            customer = Object.FindAnyObjectByType<CustomerDisplay>();

        if (customer == null) return;
        customer.SetEmotion(YorkiEmotionKeys.Neutral);
        customer.StopTalking();
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
