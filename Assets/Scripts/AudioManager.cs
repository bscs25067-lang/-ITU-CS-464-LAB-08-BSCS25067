using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Audio Sources (Routed to Mixer Groups)")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;

    [Header("UI Clips")]
    public AudioClip uiHoverClip;
    public AudioClip uiClickClip;
    public AudioClip uiBackClip;
    public AudioClip uiPanelOpenClip;
    public AudioClip uiPauseClip;
    public AudioClip uiSuccessClip;
    public AudioClip uiErrorClip;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip resultsMusic;

    private bool isUsingSourceA = true;
    private Coroutine crossfadeCoroutine;

    // PlayerPrefs Keys
    public const string KEY_MASTER = "Vol_Master";
    public const string KEY_MUSIC = "Vol_Music";
    public const string KEY_SFX = "Vol_SFX";
    public const string KEY_UI = "Vol_UI";
    public const string KEY_MUTE = "Vol_Mute";

    private bool isMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Keep UI & Music active while gameplay AudioListener.pause = true
        if (uiSource != null) uiSource.ignoreListenerPause = true;
        if (musicSourceA != null) musicSourceA.ignoreListenerPause = true;
        if (musicSourceB != null) musicSourceB.ignoreListenerPause = true;
    }

    private void Start()
    {
        LoadAudioSettings();

        if (menuMusic != null)
        {
            PlayMusic(menuMusic, 0.5f);
        }
    }

    // --- Volume & Persistence (Logarithmic dB Mapping) ---

    public void SetVolume(string exposedParam, string prefsKey, float linearValue)
    {
        linearValue = Mathf.Clamp(linearValue, 0.0001f, 1f);
        PlayerPrefs.SetFloat(prefsKey, linearValue);
        PlayerPrefs.Save();

        if (!isMuted)
        {
            ApplyMixerVolume(exposedParam, linearValue);
        }
    }

    public void SetMute(bool muted)
    {
        isMuted = muted;
        PlayerPrefs.SetInt(KEY_MUTE, muted ? 1 : 0);
        PlayerPrefs.Save();

        if (isMuted)
        {
            mainMixer.SetFloat("MasterVol", -80f);
        }
        else
        {
            ApplyMixerVolume("MasterVol", PlayerPrefs.GetFloat(KEY_MASTER, 0.8f));
        }
    }

    public bool IsMuted() => isMuted;

    public void LoadAudioSettings()
    {
        isMuted = PlayerPrefs.GetInt(KEY_MUTE, 0) == 1;

        float master = PlayerPrefs.GetFloat(KEY_MASTER, 0.8f);
        float music = PlayerPrefs.GetFloat(KEY_MUSIC, 0.7f);
        float sfx = PlayerPrefs.GetFloat(KEY_SFX, 0.8f);
        float ui = PlayerPrefs.GetFloat(KEY_UI, 0.8f);

        ApplyMixerVolume("MusicVol", music);
        ApplyMixerVolume("SFXVol", sfx);
        ApplyMixerVolume("UIVol", ui);
        ApplyMixerVolume("MasterVol", isMuted ? 0.0001f : master);
    }

    private void ApplyMixerVolume(string exposedParam, float linearValue)
    {
        float dB = linearValue <= 0.0001f ? -80f : Mathf.Log10(linearValue) * 20f;
        mainMixer.SetFloat(exposedParam, dB);
    }

    // --- UI Audio (Reusable, No Per-Button AudioSources) ---

    public void PlayUI(AudioClip clip, float pitchVariation = 0f)
    {
        if (clip == null || uiSource == null) return;
        uiSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        uiSource.PlayOneShot(clip);
    }

    public void PlayHover() => PlayUI(uiHoverClip, 0.03f);
    public void PlayClick() => PlayUI(uiClickClip, 0.02f);
    public void PlayBack() => PlayUI(uiBackClip);
    public void PlayPanelOpen() => PlayUI(uiPanelOpenClip);
    public void PlayPause() => PlayUI(uiPauseClip);
    public void PlaySuccess() => PlayUI(uiSuccessClip);
    public void PlayError() => PlayUI(uiErrorClip);

    // --- Gameplay SFX (PlayOneShot + Pitch Variation) ---

    public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitchRange = 0.08f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.pitch = 1f + Random.Range(-pitchRange, pitchRange);
        sfxSource.PlayOneShot(clip, volumeScale);
    }

    // --- Music Crossfade ---

    public void PlayMusic(AudioClip newTrack, float fadeDuration = 1.0f)
    {
        if (newTrack == null) return;

        AudioSource active = isUsingSourceA ? musicSourceA : musicSourceB;
        if (active.clip == newTrack && active.isPlaying) return;

        if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
        crossfadeCoroutine = StartCoroutine(CrossfadeMusicRoutine(newTrack, fadeDuration));
    }

    private IEnumerator CrossfadeMusicRoutine(AudioClip newTrack, float duration)
    {
        AudioSource outgoing = isUsingSourceA ? musicSourceA : musicSourceB;
        AudioSource incoming = isUsingSourceA ? musicSourceB : musicSourceA;
        isUsingSourceA = !isUsingSourceA;

        incoming.clip = newTrack;
        incoming.loop = true;
        incoming.volume = 0f;
        incoming.Play();

        float elapsed = 0f;
        float startOutVol = outgoing.isPlaying ? outgoing.volume : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            outgoing.volume = Mathf.Lerp(startOutVol, 0f, t);
            incoming.volume = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        outgoing.Stop();
        outgoing.volume = 0f;
        incoming.volume = 1f;
    }
}