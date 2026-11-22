using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Sliders (Min: 0.0001, Max: 1)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;
    [SerializeField] private Toggle muteToggle;

    [Header("Optional Percentage Labels")]
    [SerializeField] private TMP_Text masterLabel;
    [SerializeField] private TMP_Text musicLabel;
    [SerializeField] private TMP_Text sfxLabel;
    [SerializeField] private TMP_Text uiLabel;

    private void OnEnable()
    {
        // Load saved values without firing listeners prematurely
        float master = PlayerPrefs.GetFloat(AudioManager.KEY_MASTER, 0.8f);
        float music = PlayerPrefs.GetFloat(AudioManager.KEY_MUSIC, 0.7f);
        float sfx = PlayerPrefs.GetFloat(AudioManager.KEY_SFX, 0.8f);
        float ui = PlayerPrefs.GetFloat(AudioManager.KEY_UI, 0.8f);
        bool muted = PlayerPrefs.GetInt(AudioManager.KEY_MUTE, 0) == 1;

        if (masterSlider) masterSlider.SetValueWithoutNotify(master);
        if (musicSlider) musicSlider.SetValueWithoutNotify(music);
        if (sfxSlider) sfxSlider.SetValueWithoutNotify(sfx);
        if (uiSlider) uiSlider.SetValueWithoutNotify(ui);
        if (muteToggle) muteToggle.SetIsOnWithoutNotify(muted);

        UpdateLabels(master, music, sfx, ui);

        if (masterSlider) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (musicSlider) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        if (uiSlider) uiSlider.onValueChanged.AddListener(OnUIChanged);
        if (muteToggle) muteToggle.onValueChanged.AddListener(OnMuteChanged);
    }

    private void OnDisable()
    {
        if (masterSlider) masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        if (musicSlider) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
        if (uiSlider) uiSlider.onValueChanged.RemoveListener(OnUIChanged);
        if (muteToggle) muteToggle.onValueChanged.RemoveListener(OnMuteChanged);
    }

    private void OnMasterChanged(float val)
    {
        AudioManager.Instance?.SetVolume("MasterVol", AudioManager.KEY_MASTER, val);
        if (masterLabel) masterLabel.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    private void OnMusicChanged(float val)
    {
        AudioManager.Instance?.SetVolume("MusicVol", AudioManager.KEY_MUSIC, val);
        if (musicLabel) musicLabel.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    private void OnSFXChanged(float val)
    {
        AudioManager.Instance?.SetVolume("SFXVol", AudioManager.KEY_SFX, val);
        if (sfxLabel) sfxLabel.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    private void OnUIChanged(float val)
    {
        AudioManager.Instance?.SetVolume("UIVol", AudioManager.KEY_UI, val);
        if (uiLabel) uiLabel.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    private void OnMuteChanged(bool isMuted)
    {
        AudioManager.Instance?.SetMute(isMuted);
    }

    private void UpdateLabels(float m, float mu, float s, float u)
    {
        if (masterLabel) masterLabel.text = $"{Mathf.RoundToInt(m * 100)}%";
        if (musicLabel) musicLabel.text = $"{Mathf.RoundToInt(mu * 100)}%";
        if (sfxLabel) sfxLabel.text = $"{Mathf.RoundToInt(s * 100)}%";
        if (uiLabel) uiLabel.text = $"{Mathf.RoundToInt(u * 100)}%";
    }
}