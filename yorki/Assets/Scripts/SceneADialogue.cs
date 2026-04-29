using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct SceneALine
{
    public string text;
    public string emotion;  // neutral / happy / surprised / gesture / thinking
    public bool   shake;
}

public class SceneADialogue : MonoBehaviour
{
    [Header("References")]
    public CustomerDisplay customerDisplay;
    public Text            dialogueText;
    public Text            continueArrow;   // ▼ 오브젝트

    [Header("Settings")]
    public float typeSpeed      = 0.04f;
    public float poseDelay      = 0.35f;    // 포즈 그룹 전환 시 대기 시간
    public float arrowBlinkRate = 0.5f;     // ▼ 깜빡임 간격

    // ── 대사 데이터 (20줄) — 포즈 그룹 규칙 적용 ──────────
    readonly SceneALine[] _lines = new SceneALine[]
    {
        // 등장 — resting
        new SceneALine { text = "안녕하세요.",                          emotion = "neutral",  shake = false },
        new SceneALine { text = "잘 부탁드려요.",                        emotion = "neutral",  shake = false },

        // resting 블록
        new SceneALine { text = "오늘 날씨 좋죠?",                      emotion = "neutral",  shake = false },
        new SceneALine { text = "긴장되네요, 처음이라서.",               emotion = "neutral",  shake = false },
        new SceneALine { text = "잘 그려주실 거죠?",                     emotion = "happy",    shake = false },
        new SceneALine { text = "저 나중에 사진 찍어도 될까요?",         emotion = "happy",    shake = false },
        new SceneALine { text = "천천히 해도 괜찮아요.",                 emotion = "neutral",  shake = false },

        // gesture 블록 (팔 올린 상태 연속)
        new SceneALine { text = "여기서 자주 하세요?",                   emotion = "gesture",  shake = false },
        new SceneALine { text = "오래 걸려요?",                          emotion = "gesture",  shake = false },
        new SceneALine { text = "어떤 스타일로 그려주세요.",             emotion = "gesture",  shake = false },

        // thinking 단독
        new SceneALine { text = "저 어떤 표정 짓고 있으면 될까요?",     emotion = "thinking", shake = false },

        // resting 블록
        new SceneALine { text = "조용히 있을게요.",                      emotion = "neutral",  shake = false },

        // 결과 반응 — 잘 그렸을 때
        new SceneALine { text = "와...!",                                emotion = "surprised", shake = true  },
        new SceneALine { text = "진짜 저예요?",                          emotion = "happy",    shake = false },
        new SceneALine { text = "너무 잘 그려주셨어요.",                 emotion = "happy",    shake = false },
        new SceneALine { text = "친구들한테 꼭 보여줘야겠다.",           emotion = "gesture",  shake = false },

        // 결과 반응 — 못 그렸을 때
        new SceneALine { text = "...음.",                                emotion = "neutral",  shake = false },
        new SceneALine { text = "뭐... 나쁘지 않네요.",                  emotion = "neutral",  shake = false },
        new SceneALine { text = "저 이렇게 생겼나요?",                   emotion = "neutral",  shake = false },
        new SceneALine { text = "다시 해줄 수 있어요?",                  emotion = "neutral",  shake = false },
    };

    int       _index = 0;
    bool      _typing = false;
    Coroutine _typeCoroutine;
    Coroutine _blinkCoroutine;

    void Start()
    {
        SetArrowVisible(false);
        StartCoroutine(ShowLineRoutine(_index));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            Advance();
    }

    void Advance()
    {
        if (_typing)
        {
            // 타이핑 중 → 즉시 전체 출력
            if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
            dialogueText.text = _lines[_index].text;
            customerDisplay.StopTalking();
            _typing = false;
            StartBlink();
            return;
        }

        StopBlink();
        _index++;
        if (_index < _lines.Length)
            StartCoroutine(ShowLineRoutine(_index));
        else
            OnDialogueEnd();
    }

    IEnumerator ShowLineRoutine(int i)
    {
        var line = _lines[i];

        // 포즈 그룹 전환 시 짧은 pause
        if (i > 0 && GetPoseGroup(line.emotion) != GetPoseGroup(_lines[i - 1].emotion))
        {
            customerDisplay.StopTalking();
            yield return new WaitForSeconds(poseDelay);
        }

        customerDisplay.SetEmotion(line.emotion);
        customerDisplay.StartTalking(line.emotion);

        if (line.shake)
            customerDisplay.Shake();

        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _typeCoroutine = StartCoroutine(TypeLine(line.text));
    }

    IEnumerator TypeLine(string text)
    {
        _typing = true;
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        customerDisplay.StopTalking();
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
        customerDisplay.StopTalking();
        dialogueText.text = "";
        SetArrowVisible(false);
        Debug.Log("[SceneADialogue] 대사 종료");
    }

    static string GetPoseGroup(string emotion)
    {
        switch (emotion)
        {
            case "neutral":
            case "happy":    return "resting";
            case "gesture":  return "gesture";
            case "thinking": return "thinking";
            default:         return "reaction";  // surprised 등
        }
    }
}
