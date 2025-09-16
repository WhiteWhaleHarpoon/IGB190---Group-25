using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class OptionsWindow : UIWindow, IPausing, ICloseable
{
    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown qualitySettingsDropdown;

    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider effectVolumeSlider;

    [Header("Toggles")]
    [SerializeField] private Toggle showTutorialMessagesToggle;
    [SerializeField] private Toggle showDamageNumbersToggle;
    [SerializeField] private Toggle showFullHealthBarsToggle;
    [SerializeField] private Toggle showGoldPickupUIToggle;
    [SerializeField] private Toggle showTopHealthBarUIToggle;
    [SerializeField] private Toggle showGameUIToggle;

    public override void Setup()
    {
        base.Setup();
        InitializeQualitySettingsDropdown();
        InitializeSliders();
        InitializeToggles();
    }

    public void ResetKeybinds ()
    {
        GameManager.settings.keybindings = new KeyCode[8]
        {
            KeyCode.Mouse0,
            KeyCode.Mouse1,
            KeyCode.Q,
            KeyCode.W,
            KeyCode.E,
            KeyCode.R,
            KeyCode.Space,
            KeyCode.LeftShift
        };
        KeybindItem[] items = GameObject.FindObjectsByType<KeybindItem>(FindObjectsSortMode.None);
        foreach (KeybindItem item in items)
            item.UpdateKeybindDisplay();
        AbilitySlotUI[] slots = GameObject.FindObjectsByType<AbilitySlotUI>(FindObjectsSortMode.None);
        foreach (AbilitySlotUI slot in slots)
        {
            slot.RedrawHotkey();
        }

    }

    /// <summary>
    /// Initializes the quality settings dropdown.
    /// </summary>
    private void InitializeQualitySettingsDropdown()
    {
        qualitySettingsDropdown.ClearOptions();
        qualitySettingsDropdown.AddOptions(QualitySettings.names.ToList());

        int qualityIndex = Mathf.Min(QualitySettings.names.Length - 1, GameManager.settings.graphicsQuality);
        qualitySettingsDropdown.value = qualityIndex;
        QualitySettings.SetQualityLevel(qualityIndex);

        qualitySettingsDropdown.onValueChanged.AddListener(value =>
        {
            GameManager.settings.graphicsQuality = value;
            QualitySettings.SetQualityLevel(value);
        });
    }

    /// <summary>
    /// Initializes the volume sliders.
    /// </summary>
    private void InitializeSliders()
    {
        masterVolumeSlider.value = GameManager.settings.masterVolume;
        AudioListener.volume = GameManager.settings.masterVolume;
        masterVolumeSlider.onValueChanged.AddListener(value =>
        {
            AudioListener.volume = value;
            GameManager.settings.masterVolume = value;
        });

        musicVolumeSlider.value = GameManager.settings.musicVolume;
        GameManager.music.SetVolume(GameManager.settings.musicVolume);
        musicVolumeSlider.onValueChanged.AddListener(value =>
        {
            GameManager.music.SetVolume(value);
            GameManager.settings.musicVolume = value;
        });

        effectVolumeSlider.value = GameManager.settings.effectsVolume;
        effectVolumeSlider.onValueChanged.AddListener(value =>
        {
            GameManager.settings.effectsVolume = value;
        });
    }

    /// <summary>
    /// Initializes the UI toggles.
    /// </summary>
    private void InitializeToggles()
    {
        SetupToggle(showTutorialMessagesToggle, GameManager.settings.showTutorialMessages, value =>
        {
            GameManager.settings.showTutorialMessages = value;
        });

        SetupToggle(showDamageNumbersToggle, GameManager.settings.showDamageNumbers, value =>
        {
            GameManager.settings.showDamageNumbers = value;
        });

        SetupToggle(showFullHealthBarsToggle, GameManager.settings.showFullHealthBars, value =>
        {
            GameManager.settings.showFullHealthBars = value;
        });

        SetupToggle(showGoldPickupUIToggle, GameManager.settings.showGoldPickupUI, value =>
        {
            GameManager.settings.showGoldPickupUI = value;
        });

        SetupToggle(showTopHealthBarUIToggle, GameManager.settings.showTopHealthBarUI, value =>
        {
            GameManager.settings.showTopHealthBarUI = value;
        });

        SetupToggle(showGameUIToggle, GameManager.settings.showGameUI, value =>
        {
            GameManager.settings.showGameUI = value;
        });
    }

    /// <summary>
    /// Sets up a toggle with its initial value and listener.
    /// </summary>
    private void SetupToggle(Toggle toggle, bool initialValue, UnityEngine.Events.UnityAction<bool> onValueChanged)
    {
        toggle.isOn = initialValue;
        toggle.onValueChanged.AddListener(onValueChanged);
    }
}