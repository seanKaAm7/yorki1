using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerDisplay : MonoBehaviour
{
    [Header("Neutral")]
    public Sprite neutralIdle;
    public Sprite neutralTalk;   // 폴백용 (talk1/talk2 없을 때)

    [Header("Neutral Talk (토글)")]
    public Sprite talk1;         // 입 크게 — Talk1
    public Sprite talk2;         // 입 살짝 — Talk2

    [Header("Happy")]
    public Sprite happyIdle;

    [Header("Surprised")]
    public Sprite surprised;

    [Header("Gesture")]
    public Sprite gestureIdle;
    public Sprite gestureTalk;

    [Header("Settings")]
    public float talkFrameInterval = 0.10f;
    public float gestureTalkFrameInterval = 0.12f;
    public float shakeMagnitude = 6f;
    public float shakeDuration = 0.25f;

    Image _img;
    Coroutine _talkCoroutine;
    string _currentEmotion = "neutral";
    Vector2 _originPos;

    void Awake()
    {
        _img = GetComponent<Image>();
        _originPos = GetComponent<RectTransform>().anchoredPosition;
    }

    // 대사 시작 시 호출
    public void StartTalking(string emotion)
    {
        _currentEmotion = emotion;
        if (_talkCoroutine != null) StopCoroutine(_talkCoroutine);

        if (emotion == "neutral")
        {
            _talkCoroutine = StartCoroutine(NeutralTalkLoop());
            return;
        }

        if (emotion == "gesture")
        {
            _talkCoroutine = StartCoroutine(GestureTalkLoop());
            return;
        }

        SetEmotionSprite(emotion);
        _talkCoroutine = null;
    }

    // 대사 끝 시 호출
    public void StopTalking()
    {
        if (_talkCoroutine != null) StopCoroutine(_talkCoroutine);
        _talkCoroutine = null;
        SetEmotionSprite(_currentEmotion);
    }

    // 즉시 표정 전환
    public void SetEmotion(string emotion)
    {
        _currentEmotion = emotion;
        SetEmotionSprite(emotion);
    }

    // 강조 흔들기
    public void Shake()
    {
        StartCoroutine(ShakeCoroutine());
    }

    void OnDisable()
    {
        if (_talkCoroutine != null) StopCoroutine(_talkCoroutine);
        _talkCoroutine = null;
    }

    IEnumerator NeutralTalkLoop()
    {
        Sprite[] frames = new Sprite[]
        {
            talk2 != null ? talk2 : GetTalkSprite("neutral"),
            neutralTalk != null ? neutralTalk : GetTalkSprite("neutral"),
            talk1 != null ? talk1 : GetTalkSprite("neutral"),
            neutralTalk != null ? neutralTalk : GetTalkSprite("neutral"),
        };

        int index = 0;
        while (true)
        {
            _img.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(talkFrameInterval);
        }
    }

    IEnumerator GestureTalkLoop()
    {
        Sprite idle = gestureIdle != null ? gestureIdle : neutralIdle;
        Sprite talk = gestureTalk != null ? gestureTalk : idle;
        bool useTalk = false;

        while (true)
        {
            _img.sprite = useTalk ? talk : idle;
            useTalk = !useTalk;
            yield return new WaitForSeconds(gestureTalkFrameInterval);
        }
    }

    IEnumerator ShakeCoroutine()
    {
        var rt = GetComponent<RectTransform>();
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = Random.Range(-shakeMagnitude * 0.5f, shakeMagnitude * 0.5f);
            rt.anchoredPosition = _originPos + new Vector2(x, y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = _originPos;
    }

    void SetEmotionSprite(string emotion)
    {
        _img.sprite = GetIdleSprite(emotion);
    }

    Sprite GetIdleSprite(string emotion)
    {
        switch (emotion)
        {
            case "happy":     return happyIdle;
            case "surprised": return surprised;
            case "gesture":   return gestureIdle;
            default:          return neutralIdle;
        }
    }

    Sprite GetTalkSprite(string emotion)
    {
        switch (emotion)
        {
            case "happy":   return happyIdle;
            case "gesture": return gestureTalk != null ? gestureTalk : gestureIdle;
            default:        return neutralTalk != null ? neutralTalk : neutralIdle;
        }
    }
}
