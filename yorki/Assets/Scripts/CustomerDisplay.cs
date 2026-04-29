using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerDisplay : MonoBehaviour
{
    [Header("Neutral")]
    public Sprite neutralIdle;
    public Sprite neutralTalk;

    [Header("Happy")]
    public Sprite happyIdle;

    [Header("Surprised")]
    public Sprite surprised;

    [Header("Gesture")]
    public Sprite gestureIdle;
    public Sprite gestureTalk;

    [Header("Settings")]
    public float talkFrameInterval = 0.12f;
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
        SetEmotionSprite(emotion);
        if (_talkCoroutine != null) StopCoroutine(_talkCoroutine);
        _talkCoroutine = StartCoroutine(TalkLoop(emotion));
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

    IEnumerator TalkLoop(string emotion)
    {
        bool toggle = false;
        while (true)
        {
            _img.sprite = toggle ? GetIdleSprite(emotion) : GetTalkSprite(emotion);
            toggle = !toggle;
            yield return new WaitForSeconds(talkFrameInterval);
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
