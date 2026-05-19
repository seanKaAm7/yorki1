using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomerEpisode", menuName = "Yorki/Customer Episode")]
public class CustomerEpisodeData : ScriptableObject
{
    [Header("식별")]
    public string customerId;
    public string customerName;

    [Header("표정 컷 (선택 — 비워두면 CustomerDisplay 컴포넌트 기본값 유지)")]
    public Sprite neutralIdle;
    public Sprite neutralTalk;
    public Sprite talk1;
    public Sprite talk2;
    public Sprite happyIdle;
    public Sprite surprised;
    public Sprite gestureIdle;
    public Sprite gestureTalk;

    [Header("대사")]
    public DialogueLineData[] preDrawLines;
    public DialogueLineData[] goodResultLines;
    public DialogueLineData[] badResultLines;
}
