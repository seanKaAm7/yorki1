using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TalkScenePhase
{
    PreDraw,
    ResultGood,
    ResultBad
}

[System.Serializable]
public struct TalkSceneLine
{
    public string text;
    public string emotion;
    public bool shake;
}

public class TalkSceneController : MonoBehaviour
{
    [Header("References")]
    public CustomerDisplay customerDisplay;
    public Text dialogueText;
    public Text continueArrow;
    public CanvasGroup dialogueBoxGroup;
    public SceneTransition sceneTransition;

    [Header("Settings")]
    public TalkScenePhase phase = TalkScenePhase.PreDraw;
    public bool useGameManagerPhase = true;
    public float typeSpeed = 0.04f;
    public float poseDelay = 0.35f;
    public float arrowBlinkRate = 0.5f;

    readonly TalkSceneLine[] _preDrawLines = new TalkSceneLine[]
    {
        new TalkSceneLine { text = "안녕하세요.", emotion = "neutral", shake = false },
        new TalkSceneLine { text = "잘 부탁드려요.", emotion = "neutral", shake = false },
        new TalkSceneLine { text = "오늘 날씨 좋죠?", emotion = "neutral", shake = false },
        new TalkSceneLine { text = "긴장되네요, 처음이라서.", emotion = "neutral", shake = false },
        new TalkSceneLine { text = "잘 그려주실 거죠?", emotion = "happy", shake = false },
        new TalkSceneLine { text = "저 나중에 사진 찍어도 될까요?", emotion = "happy", shake = false },
        new TalkSceneLine { text = "천천히 해도 괜찮아요.", emotion = "neutral", shake = false },
        new TalkSceneLine { text = "여기서 자주 하세요?", emotion = "gesture", shake = false },
        new TalkSceneLine { text = "오래 걸려요?", emotion = "gesture", shake = false },
        new TalkSceneLine { text = "어떤 스타일로 그려주세요.", emotion = "gesture", shake = false },
        new TalkSceneLine { text = "저 어떤 표정 짓고 있으면 될까요?", emotion = "thinking", shake = false },
        new TalkSceneLine { text = "조용히 있을게요.", emotion = "neutral", shake = false },
    };

    readonly TalkSceneLine[] _resultGoodLines = new TalkSceneLine[]
    {
        new TalkSceneLine { text = "와...!", emotion = "surprised", shake = true },
        new TalkSceneLine { text = "진짜 저예요?", emotion = "happy", shake = false },
        new TalkSceneLine { text = "너무 잘 그려주셨어요.", emotion = "happy", shake = false },
        new TalkSceneLine { text = "친구들한테 꼭 보여줘야겠다.", emotion = "gesture", shake = false },
    };

    readonly TalkSceneLine[] _resultBadLines = new TalkSceneLine[]
    {
        new TalkSceneLine { text = "...음.", emotion = "neutral", shake = false },
        new TalkSceneLine { text = "뭐... 나쁘지 않네요.", emotion = "neutral", shake = false },
        new TalkSceneLine { text = "저 이렇게 생겼나요?", emotion = "neutral", shake = false },
        new TalkSceneLine { text = "다시 해줄 수 있어요?", emotion = "neutral", shake = false },
    };

    TalkSceneLine[] _lines;
    int _index;
    bool _typing;
    bool _ended;
    Coroutine _typeCoroutine;
    Coroutine _blinkCoroutine;

    void Start()
    {
        BindReferences();

        if (useGameManagerPhase)
            phase = GameManager.currentTalkPhase;

        _lines = GetLinesForPhase(phase);
        _index = 0;

        SetArrowVisible(false);
        if (dialogueBoxGroup != null && !sceneTransition.IsTransitioning)
            dialogueBoxGroup.alpha = 1f;

        if (!sceneTransition.IsTransitioning)
            sceneTransition.PlaceStageForTalkScene();

        if (_lines.Length > 0)
            StartCoroutine(ShowLineRoutine(_index));
    }

    void Update()
    {
        if (_ended) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            Advance();
    }

    void BindReferences()
    {
        if (sceneTransition == null)
            sceneTransition = SceneTransition.EnsureInstance();

        if (customerDisplay == null && PersistentBootstrap.Instance != null)
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
        TalkSceneLine line = _lines[i];

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
            sceneTransition.TalkSceneToSceneA();
            return;
        }

        Debug.Log("[TalkSceneController] 결과 대사 종료");
    }

    TalkSceneLine[] GetLinesForPhase(TalkScenePhase targetPhase)
    {
        switch (targetPhase)
        {
            case TalkScenePhase.ResultGood: return _resultGoodLines;
            case TalkScenePhase.ResultBad: return _resultBadLines;
            default: return _preDrawLines;
        }
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
