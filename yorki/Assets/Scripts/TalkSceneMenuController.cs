using UnityEngine;
using UnityEngine.UI;

public class TalkSceneMenuController : MonoBehaviour
{
    static readonly Color SpeedSelectedColor = new Color(0.70f, 0.47f, 0.24f, 1f);
    static readonly Color SpeedIdleColor = new Color(0.16f, 0.14f, 0.12f, 0.96f);

    public static bool IsAnyMenuOpen { get; private set; }

    [Header("HUD")]
    public TalkSceneHUDController hud;
    public Button settingsButton;
    public Button recordsButton;

    [Header("Modal")]
    public GameObject modalLayer;
    public GameObject settingsPanel;
    public GameObject recordsPanel;
    public Button modalDimButton;
    public Button settingsCloseButton;
    public Button recordsCloseButton;

    [Header("Settings")]
    public Slider masterVolumeSlider;
    public Text masterVolumeValueText;
    public Button slowSpeedButton;
    public Button normalSpeedButton;
    public Button fastSpeedButton;
    public Toggle fullscreenToggle;

    [Header("Records")]
    public PortraitRecordsPanelController recordsPanelController;

    void Awake()
    {
        YorkiSettingsService.Apply();

        settingsButton?.onClick.AddListener(OpenSettings);
        recordsButton?.onClick.AddListener(OpenRecords);
        modalDimButton?.onClick.AddListener(CloseMenu);
        settingsCloseButton?.onClick.AddListener(CloseMenu);
        recordsCloseButton?.onClick.AddListener(CloseMenu);
        masterVolumeSlider?.onValueChanged.AddListener(SetMasterVolume);
        slowSpeedButton?.onClick.AddListener(() => SetDialogueSpeed(YorkiDialogueSpeed.Slow));
        normalSpeedButton?.onClick.AddListener(() => SetDialogueSpeed(YorkiDialogueSpeed.Normal));
        fastSpeedButton?.onClick.AddListener(() => SetDialogueSpeed(YorkiDialogueSpeed.Fast));
        fullscreenToggle?.onValueChanged.AddListener(SetFullscreen);

        RefreshSettingsUI();
        CloseMenu();
    }

    void Update()
    {
        if (IsAnyMenuOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseMenu();
    }

    public void OpenSettings()
    {
        if (!CanOpenMenu())
            return;

        OpenPanel(settingsPanel);
        RefreshSettingsUI();
    }

    public void OpenRecords()
    {
        if (!CanOpenMenu())
            return;

        OpenPanel(recordsPanel);
        recordsPanelController?.RefreshRecords();
    }

    public void CloseMenu()
    {
        IsAnyMenuOpen = false;
        if (modalLayer != null)
            modalLayer.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (recordsPanel != null)
            recordsPanel.SetActive(false);
    }

    void OpenPanel(GameObject panel)
    {
        hud?.HideStatTooltip();
        IsAnyMenuOpen = true;
        if (modalLayer != null)
            modalLayer.SetActive(true);
        if (settingsPanel != null)
            settingsPanel.SetActive(panel == settingsPanel);
        if (recordsPanel != null)
            recordsPanel.SetActive(panel == recordsPanel);
    }

    bool CanOpenMenu()
    {
        return SceneTransition.Instance == null || !SceneTransition.Instance.IsTransitioning;
    }

    void SetMasterVolume(float value)
    {
        YorkiSettingsService.SetMasterVolume(value / 100f);
        RefreshMasterVolumeText();
    }

    void SetDialogueSpeed(YorkiDialogueSpeed speed)
    {
        YorkiSettingsService.SetDialogueSpeed(speed);
        YorkiSettingsService.ApplyDialogueSpeed(Object.FindAnyObjectByType<TalkSceneController>());
        RefreshSpeedButtons();
    }

    void SetFullscreen(bool value)
    {
        YorkiSettingsService.SetFullscreen(value);
    }

    void RefreshSettingsUI()
    {
        YorkiSettingsData settings = YorkiSettingsService.Current;
        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(settings.masterVolume * 100f);
        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(settings.fullscreen);

        RefreshMasterVolumeText();
        RefreshSpeedButtons();
    }

    void RefreshMasterVolumeText()
    {
        if (masterVolumeValueText != null)
            masterVolumeValueText.text = $"{Mathf.RoundToInt(YorkiSettingsService.Current.masterVolume * 100f)}";
    }

    void RefreshSpeedButtons()
    {
        YorkiDialogueSpeed speed = YorkiSettingsService.Current.dialogueSpeed;
        SetButtonColor(slowSpeedButton, speed == YorkiDialogueSpeed.Slow);
        SetButtonColor(normalSpeedButton, speed == YorkiDialogueSpeed.Normal);
        SetButtonColor(fastSpeedButton, speed == YorkiDialogueSpeed.Fast);
    }

    static void SetButtonColor(Button button, bool selected)
    {
        if (button != null && button.targetGraphic != null)
            button.targetGraphic.color = selected ? SpeedSelectedColor : SpeedIdleColor;
    }

    void OnDestroy()
    {
        IsAnyMenuOpen = false;
    }
}
