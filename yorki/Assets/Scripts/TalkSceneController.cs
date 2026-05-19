using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TalkScenePhase
{
    PreDraw,
    ResultGood,
    ResultBad
}

public class TalkSceneController : MonoBehaviour
{
    [Header("References")]
    public CustomerDisplay customerDisplay;
    public Text dialogueText;
    public Text continueArrow;
    public CanvasGroup dialogueBoxGroup;
    public SceneTransition sceneTransition;

    [Header("Episode")]
    [Tooltip("현재 진행 중인 손님. 게임 시작 시 GameManager 큐의 첫 손님으로 자동 교체됨.")]
    public CustomerEpisodeData currentEpisode;

    [Tooltip("게임 첫 시작 시 GameManager 큐에 시드할 손님 순서. GameManager가 이미 있으면 무시됨.")]
    public CustomerEpisodeData[] dayEpisodeQueue;

    [Tooltip("결과 대사 끝 ~ 다음 손님 등장까지 텀(초). 페이드아웃/페이드인 포함.")]
    public float interCustomerDelay = 6f;
    public float customerFadeDuration = 1f;

    [Header("Settings")]
    public TalkScenePhase phase = TalkScenePhase.PreDraw;
    public bool useGameManagerPhase = true;
    public float typeSpeed = 0.04f;
    public float poseDelay = 0.35f;
    public float arrowBlinkRate = 0.5f;

    DialogueLineData[] _lines;
    int _index;
    bool _typing;
    bool _ended;
    Coroutine _typeCoroutine;
    Coroutine _blinkCoroutine;

    void Start()
    {
        BindReferences();
        EnsureGameManager();

        // GameManager의 큐가 살아 있으면 인스펙터 currentEpisode를 덮어쓴다.
        if (GameManager.Instance != null && GameManager.Instance.CurrentEpisode != null)
            currentEpisode = GameManager.Instance.CurrentEpisode;

        ApplyEpisodeSprites();

        if (useGameManagerPhase)
            phase = GameManager.currentTalkPhase;

        _lines = GetLinesForPhase(phase);
        _index = 0;

        SetArrowVisible(false);
        if (dialogueBoxGroup != null && !sceneTransition.IsTransitioning)
            dialogueBoxGroup.alpha = 1f;

        if (!sceneTransition.IsTransitioning)
            sceneTransition.PlaceStageForTalkScene();

        if (_lines != null && _lines.Length > 0)
            StartCoroutine(ShowLineRoutine(_index));
        else
            OnDialogueEnd();
    }

    void Update()
    {
        if (_ended) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            Advance();
    }

    void BindReferences()
    {
        // SceneTransition / CustomerDisplay는 영속 싱글턴이다.
        // 빌더가 만든 새 인스턴스는 Awake에서 자기를 Destroy하므로,
        // 인스펙터에 박힌 참조는 곧 파괴될 객체를 가리킨다.
        // 항상 살아있는 싱글턴으로 재바인딩한다.
        SceneTransition persistentTransition = SceneTransition.EnsureInstance();
        if (persistentTransition != null)
            sceneTransition = persistentTransition;

        if (PersistentBootstrap.Instance != null && PersistentBootstrap.Instance.customerDisplay != null)
            customerDisplay = PersistentBootstrap.Instance.customerDisplay;

        if (customerDisplay == null)
            customerDisplay = Object.FindAnyObjectByType<CustomerDisplay>();

        if (dialogueText == null)
        {
            GameObject go = GameObject.Find("DialogueText");
            if (go != null) dialogueText = go.GetComponent<Text>();
        }

        if (continueArrow == null)
        {
            GameObject go = GameObject.Find("ContinueArrow");
            if (go != null) continueArrow = go.GetComponent<Text>();
        }

        if (dialogueBoxGroup == null)
        {
            GameObject go = GameObject.Find("DialogueBox");
            if (go != null) dialogueBoxGroup = SceneTransition.EnsureCanvasGroup(go);
        }
    }

    void Advance()
    {
        if (_typing)
        {
            if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
            if (dialogueText != null)
                dialogueText.text = _lines[_index].text;
            customerDisplay?.StopTalking();
            _typing = false;
            StartBlink();
            return;
        }

        StopBlink();
        _index++;

        if (_index < _lines.Length)
        {
            StartCoroutine(ShowLineRoutine(_index));
            return;
        }

        OnDialogueEnd();
    }

    IEnumerator ShowLineRoutine(int i)
    {
        DialogueLineData line = _lines[i];

        if (i > 0 && GetPoseGroup(line.emotion) != GetPoseGroup(_lines[i - 1].emotion))
        {
            customerDisplay?.StopTalking();
            yield return new WaitForSeconds(poseDelay);
        }

        customerDisplay?.SetEmotion(line.emotion);
        customerDisplay?.StartTalking(line.emotion);

        if (line.shake)
            customerDisplay?.Shake();

        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _typeCoroutine = StartCoroutine(TypeLine(line.text));
    }

    IEnumerator TypeLine(string text)
    {
        _typing = true;
        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char c in text)
        {
            if (dialogueText != null)
                dialogueText.text += c;
            if (ShouldMoveMouth(c))
                customerDisplay?.AdvanceTalkFrame();
            else
                customerDisplay?.CloseMouth();
            yield return new WaitForSeconds(typeSpeed);
        }

        customerDisplay?.StopTalking();
        _typing = false;
        StartBlink();
    }

    void StartBlink()
    {
        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(BlinkArrow());
    }

    void StopBlink()
    {
        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = null;
        SetArrowVisible(false);
    }

    IEnumerator BlinkArrow()
    {
        while (true)
        {
            SetArrowVisible(true);
            yield return new WaitForSeconds(arrowBlinkRate);
            SetArrowVisible(false);
            yield return new WaitForSeconds(arrowBlinkRate);
        }
    }

    void SetArrowVisible(bool visible)
    {
        if (continueArrow != null)
            continueArrow.enabled = visible;
    }

    void OnDialogueEnd()
    {
        _ended = true;
        customerDisplay?.StopTalking();
        if (dialogueText != null)
            dialogueText.text = "";
        SetArrowVisible(false);

        if (phase == TalkScenePhase.PreDraw)
        {
            customerDisplay?.SetEmotion("neutral");
            customerDisplay?.StopTalking();
            sceneTransition.TalkSceneToSceneA();
            return;
        }

        // 결과 대사 종료 → 다음 손님으로 in-place 전환
        StartCoroutine(NextCustomerRoutine());
    }

    IEnumerator NextCustomerRoutine()
    {
        // 1) 현재 손님 페이드아웃
        yield return FadeCustomer(1f, 0f, customerFadeDuration);

        // 2) 빈 좌석 텀 (총 interCustomerDelay에서 페이드 시간 빼기)
        float idle = Mathf.Max(0f, interCustomerDelay - customerFadeDuration * 2f);
        yield return new WaitForSeconds(idle);

        // 3) 큐 진행
        if (GameManager.Instance != null)
            GameManager.Instance.AdvanceToNextEpisode();

        if (GameManager.Instance == null || GameManager.Instance.CurrentEpisode == null)
        {
            Debug.Log("[TalkSceneController] 하루 종료 — 손님 모두 응대 완료");
            yield break;
        }

        // 4) 다음 손님으로 교체 + 스프라이트 갱신
        currentEpisode = GameManager.Instance.CurrentEpisode;
        ApplyEpisodeSprites();

        // 5) 페이드인
        yield return FadeCustomer(0f, 1f, customerFadeDuration);

        // 6) PreDraw 다시 시작
        phase = TalkScenePhase.PreDraw;
        GameManager.currentTalkPhase = TalkScenePhase.PreDraw;
        _lines = GetLinesForPhase(phase);
        _index = 0;
        _ended = false;
        _typing = false;

        if (_lines != null && _lines.Length > 0)
            StartCoroutine(ShowLineRoutine(_index));
        else
            OnDialogueEnd();
    }

    IEnumerator FadeCustomer(float from, float to, float duration)
    {
        if (customerDisplay == null) yield break;
        Image img = customerDisplay.GetComponent<Image>();
        if (img == null) yield break;

        Color c = img.color;
        c.a = from;
        img.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
    }

    void EnsureGameManager()
    {
        if (GameManager.Instance != null) return;

        GameObject go = new GameObject("GameManager");
        GameManager gm = go.AddComponent<GameManager>();
        if (dayEpisodeQueue != null && dayEpisodeQueue.Length > 0)
            gm.episodeQueue = dayEpisodeQueue;
    }

    void ApplyEpisodeSprites()
    {
        if (currentEpisode == null || customerDisplay == null) return;

        if (currentEpisode.neutralIdle != null) customerDisplay.neutralIdle = currentEpisode.neutralIdle;
        if (currentEpisode.neutralTalk != null) customerDisplay.neutralTalk = currentEpisode.neutralTalk;
        if (currentEpisode.talk1       != null) customerDisplay.talk1       = currentEpisode.talk1;
        if (currentEpisode.talk2       != null) customerDisplay.talk2       = currentEpisode.talk2;
        if (currentEpisode.happyIdle   != null) customerDisplay.happyIdle   = currentEpisode.happyIdle;
        if (currentEpisode.surprised   != null) customerDisplay.surprised   = currentEpisode.surprised;
        if (currentEpisode.gestureIdle != null) customerDisplay.gestureIdle = currentEpisode.gestureIdle;
        if (currentEpisode.gestureTalk != null) customerDisplay.gestureTalk = currentEpisode.gestureTalk;

        customerDisplay.SetEmotion("neutral");
    }

    DialogueLineData[] GetLinesForPhase(TalkScenePhase targetPhase)
    {
        if (currentEpisode == null)
        {
            Debug.LogWarning("[TalkSceneController] currentEpisode가 비어 있음. Inspector에서 CustomerEpisodeData를 연결하세요.");
            return new DialogueLineData[0];
        }

        DialogueLineData[] lines;
        switch (targetPhase)
        {
            case TalkScenePhase.ResultGood: lines = currentEpisode.goodResultLines; break;
            case TalkScenePhase.ResultBad:  lines = currentEpisode.badResultLines;  break;
            default:                        lines = currentEpisode.preDrawLines;    break;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning($"[TalkSceneController] '{currentEpisode.name}' 에피소드의 {targetPhase} 대사 배열이 비어 있음.");
            return new DialogueLineData[0];
        }

        return lines;
    }

    static string GetPoseGroup(string emotion)
    {
        switch (emotion)
        {
            case "neutral":
            case "happy":
                return "resting";
            case "gesture":
                return "gesture";
            case "thinking":
                return "thinking";
            default:
                return "reaction";
        }
    }

    static bool ShouldMoveMouth(char c)
    {
        if (char.IsWhiteSpace(c))
            return false;

        switch (c)
        {
            case '.':
            case ',':
            case '?':
            case '!':
            case '…':
            case '~':
            case '-':
                return false;
            default:
                return true;
        }
    }
}
