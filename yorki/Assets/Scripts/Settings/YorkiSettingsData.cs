using System;

public enum YorkiDialogueSpeed
{
    Slow,
    Normal,
    Fast
}

[Serializable]
public class YorkiSettingsData
{
    public float masterVolume = 1f;
    public YorkiDialogueSpeed dialogueSpeed = YorkiDialogueSpeed.Normal;
    public bool fullscreen;
}
