using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TalkSceneHUDController : MonoBehaviour
{
    [Header("Header")]
    public Text placeText;
    public Text dayText;
    public Text clockText;
    public Text seasonText;
    public Text weekdayText;

    [Header("Stats")]
    public Text incomeValueText;
    public Text timeValueText;
    public Image energyFill;
    public Image reputationFill;
    public Image materialsFill;

    [Header("Stat Tooltip")]
    public RectTransform statTooltipRoot;
    public Text statTooltipText;
    public Vector2 statTooltipOffset = new Vector2(16f, -18f);

    [Header("Right Panels")]
    public Text goalTitleText;
    public Text goalBodyText;
    public Text goalProgressText;
    public Text reservationBodyText;

    TalkSceneHUDStatKind activeTooltipStat;
    bool tooltipVisible;
    string energyTooltipValue = "에너지 78 / 100";
    string reputationTooltipValue = "평판 4.5 / 5.0";
    string materialsTooltipValue = "재료 12 / 20";

    void Update()
    {
        GameManager gm = GameManager.Instance;

        string placeName = gm != null ? gm.placeName : "Zum Goldenen Hahn";
        int dayIndex = gm != null ? gm.dayIndex : 7;
        int currentHour = gm != null ? gm.currentHour : 10;
        int currentMinute = gm != null ? gm.currentMinute : 30;
        int closingHour = gm != null ? gm.closingHour : 22;
        int closingMinute = gm != null ? gm.closingMinute : 0;
        int income = gm != null ? gm.todayEarnings : 0;
        int customersServed = gm != null ? gm.customersServed : 0;
        int goalTarget = ResolveGoalTarget(gm);
        int energy = gm != null ? gm.energy : 78;
        int maxEnergy = gm != null ? gm.maxEnergy : 100;
        float reputation = gm != null ? gm.reputation : 4.5f;
        int materials = gm != null ? gm.materials : 12;
        int maxMaterials = gm != null ? gm.maxMaterials : 20;
        string goalLabel = gm != null && !string.IsNullOrWhiteSpace(gm.dailyGoalLabel)
            ? gm.dailyGoalLabel
            : "오늘 손님 3명을 맞이해보세요!";
        string reservation = gm != null && !string.IsNullOrWhiteSpace(gm.reservationLabel)
            ? gm.reservationLabel
            : "없음";

        SetText(placeText, placeName);
        SetText(dayText, $"Day {dayIndex}");
        SetText(clockText, FormatClock(currentHour, currentMinute));
        SetText(seasonText, "섬괴옥");
        SetText(weekdayText, "월요일");

        SetText(incomeValueText, $"€ {income:0}.00");
        SetText(timeValueText, $"{FormatClock(currentHour, currentMinute)} / {FormatClock(closingHour, closingMinute)}");

        SetFill(energyFill, energy, maxEnergy);
        SetFill(reputationFill, reputation, 5f);
        SetFill(materialsFill, materials, maxMaterials);

        energyTooltipValue = $"에너지 {energy} / {maxEnergy}";
        reputationTooltipValue = $"평판 {reputation:0.0} / 5.0";
        materialsTooltipValue = $"재료 {materials} / {maxMaterials}";
        if (tooltipVisible)
            SetText(statTooltipText, GetTooltipValue(activeTooltipStat));

        SetText(goalTitleText, "다음 목표");
        SetText(goalBodyText, goalLabel);
        SetText(goalProgressText, $"({Mathf.Min(customersServed, goalTarget)} / {goalTarget})");
        SetText(reservationBodyText, reservation);
    }

    public void ShowStatTooltip(TalkSceneHUDStatKind statKind, Vector2 screenPosition)
    {
        activeTooltipStat = statKind;
        tooltipVisible = true;
        SetText(statTooltipText, GetTooltipValue(statKind));
        MoveStatTooltip(screenPosition);

        if (statTooltipRoot != null)
            statTooltipRoot.gameObject.SetActive(true);
    }

    public void MoveStatTooltip(Vector2 screenPosition)
    {
        if (statTooltipRoot != null)
            statTooltipRoot.position = screenPosition + statTooltipOffset;
    }

    public void HideStatTooltip(TalkSceneHUDStatKind statKind)
    {
        if (!tooltipVisible || activeTooltipStat != statKind)
            return;

        tooltipVisible = false;
        if (statTooltipRoot != null)
            statTooltipRoot.gameObject.SetActive(false);
    }

    string GetTooltipValue(TalkSceneHUDStatKind statKind)
    {
        switch (statKind)
        {
            case TalkSceneHUDStatKind.Energy:
                return energyTooltipValue;
            case TalkSceneHUDStatKind.Reputation:
                return reputationTooltipValue;
            case TalkSceneHUDStatKind.Materials:
                return materialsTooltipValue;
            default:
                return "";
        }
    }

    static int ResolveGoalTarget(GameManager gm)
    {
        if (gm == null)
            return 3;
        if (gm.dailyGoalTarget > 0)
            return gm.dailyGoalTarget;
        if (gm.episodeQueue != null && gm.episodeQueue.Length > 0)
            return gm.episodeQueue.Length;
        return 3;
    }

    static string FormatClock(int hour, int minute)
    {
        return $"{hour:00}:{minute:00}";
    }

    static void SetText(Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    static void SetFill(Image image, float value, float max)
    {
        if (image == null)
            return;

        image.fillAmount = max > 0f ? Mathf.Clamp01(value / max) : 0f;
    }
}

public enum TalkSceneHUDStatKind
{
    Energy,
    Reputation,
    Materials
}

public class TalkSceneHUDTooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public TalkSceneHUDController hud;
    public TalkSceneHUDStatKind statKind;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hud != null)
            hud.ShowStatTooltip(statKind, eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (hud != null)
            hud.MoveStatTooltip(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hud != null)
            hud.HideStatTooltip(statKind);
    }
}
