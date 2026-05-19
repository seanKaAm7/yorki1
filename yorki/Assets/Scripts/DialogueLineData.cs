using UnityEngine;

[System.Serializable]
public class DialogueLineData
{
    [Tooltip("CustomerDisplay 감정 키: neutral / happy / surprised / gesture / thinking")]
    public string emotion = "neutral";

    [TextArea(2, 4)]
    public string text;

    public bool shake;
}
