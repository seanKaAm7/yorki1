using UnityEngine;
using UnityEngine.EventSystems;

// THICKNESS 세로 슬라이더 손잡이.
// SliderTrack(부모) 내에서 위아래로만 움직이며, 0~1 사이 값을 컨트롤러에 전달.
public class ThicknessSliderHandle : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("References")]
    public RectTransform track;
    public RectTransform handle;

    [Header("Settings")]
    public float minY = -120f;
    public float maxY = 120f;

    public System.Action<float> ValueChanged;

    public float NormalizedValue { get; private set; } = 0.5f;

    void Reset()
    {
        handle = GetComponent<RectTransform>();
    }

    void Awake()
    {
        if (handle == null) handle = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateValueFromPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateValueFromPointer(eventData);
    }

    void UpdateValueFromPointer(PointerEventData eventData)
    {
        if (track == null) return;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            track, eventData.position, eventData.pressEventCamera, out localPos);

        float clampedY = Mathf.Clamp(localPos.y, minY, maxY);
        SetHandleY(clampedY);

        float t = Mathf.InverseLerp(minY, maxY, clampedY);
        NormalizedValue = t;
        ValueChanged?.Invoke(t);
    }

    public void SetNormalizedValue(float t, bool notify = false)
    {
        t = Mathf.Clamp01(t);
        NormalizedValue = t;
        SetHandleY(Mathf.Lerp(minY, maxY, t));
        if (notify) ValueChanged?.Invoke(t);
    }

    void SetHandleY(float y)
    {
        if (handle == null) return;
        Vector2 pos = handle.anchoredPosition;
        pos.y = y;
        handle.anchoredPosition = pos;
    }
}
