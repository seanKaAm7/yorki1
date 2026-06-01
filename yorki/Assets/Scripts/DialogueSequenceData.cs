using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueSequence", menuName = "Yorki/Dialogue Sequence")]
public class DialogueSequenceData : ScriptableObject
{
    [Tooltip("각 줄의 화자명이 비어 있을 때 사용할 기본 화자명입니다.")]
    public string defaultSpeakerName;

    [Header("대사")]
    public DialogueLineData[] lines;
}
