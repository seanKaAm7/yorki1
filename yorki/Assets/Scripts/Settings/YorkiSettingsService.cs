using UnityEngine;

public static class YorkiSettingsService
{
    const string MasterVolumeKey = "Yorki.Settings.MasterVolume";
    const string DialogueSpeedKey = "Yorki.Settings.DialogueSpeed";
    const string FullscreenKey = "Yorki.Settings.Fullscreen";

    static YorkiSettingsData settings;

    public static YorkiSettingsData Current
    {
        get
        {
            EnsureLoaded();
            return settings;
        }
    }

    public static float DialogueTypeSpeed
    {
        get
        {
            switch (Current.dialogueSpeed)
            {
                case YorkiDialogueSpeed.Slow: return 0.06f;
                case YorkiDialogueSpeed.Fast: return 0.02f;
                default: return 0.04f;
            }
        }
    }

    public static void Apply()
    {
        EnsureLoaded();
        AudioListener.volume = settings.masterVolume;
        Screen.fullScreen = settings.fullscreen;
    }

    public static void ApplyDialogueSpeed(TalkSceneController controller)
    {
        if (controller != null)
            controller.typeSpeed = DialogueTypeSpeed;
    }

    public static void SetMasterVolume(float value)
    {
        Current.masterVolume = Mathf.Clamp01(value);
        AudioListener.volume = settings.masterVolume;
        Save();
    }

    public static void SetDialogueSpeed(YorkiDialogueSpeed value)
    {
        Current.dialogueSpeed = value;
        Save();
    }

    public static void SetFullscreen(bool value)
    {
        Current.fullscreen = value;
        Screen.fullScreen = value;
        Save();
    }

    static void EnsureLoaded()
    {
        if (settings != null)
            return;

        settings = new YorkiSettingsData
        {
            masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f),
            dialogueSpeed = (YorkiDialogueSpeed)PlayerPrefs.GetInt(DialogueSpeedKey, (int)YorkiDialogueSpeed.Normal),
            fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1
        };
    }

    static void Save()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, settings.masterVolume);
        PlayerPrefs.SetInt(DialogueSpeedKey, (int)settings.dialogueSpeed);
        PlayerPrefs.SetInt(FullscreenKey, settings.fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
