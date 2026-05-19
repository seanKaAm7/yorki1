using UnityEngine;

[System.Serializable]
public class DialogueLineData
{
    [Tooltip("비워두면 현재 손님 이름 또는 독백 기본 화자명이 표시됩니다.")]
    public string speakerName;

    [Tooltip("CustomerDisplay 감정 키: neutral / happy / surprised / gesture / thinking")]
    public string emotion = "neutral";

    [TextArea(2, 4)]
    public string text;

    public bool shake;
}
