using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    // Make sure this matches your exact scene filename in Build Settings
    private const string TransitionSceneName = "TransitionScene";

    [Header("Transition UI References")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private RectTransform topShutter;
    [SerializeField] private RectTransform bottomShutter;

    [Header("Lab 08 Async Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private float minimumLoadTime = 1.0f;

    [Header("Transition Settings")]
    [SerializeField] private float duration = 0.35f;

    private bool isTransitioning = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapTransitionScene()
    {
        Scene transitionScene = SceneManager.GetSceneByName(TransitionSceneName);
        if (!transitionScene.isLoaded)
        {
            SceneManager.LoadSceneAsync(TransitionSceneName, LoadSceneMode.Additive);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        if (loadingPanel != null) loadingPanel.SetActive(false);

        if (topShutter != null) topShutter.anchoredPosition = new Vector2(0f, 560f);
        if (bottomShutter != null) bottomShutter.anchoredPosition = new Vector2(0f, -560f);
    }

    /// <summary>
    /// Transitions to targetSceneName. Set showLoadingBar = true for Menu -> Gameplay loading.
    /// </summary>
    public static void TransitionTo(string targetSceneName, bool showLoadingBar = true)
    {
        if (string.IsNullOrEmpty(targetSceneName)) return;

        if (Instance == null)
        {
            SceneManager.LoadSceneAsync(TransitionSceneName, LoadSceneMode.Additive).completed += _ =>
            {
                if (Instance != null) Instance.StartTransition(targetSceneName, showLoadingBar);
            };
            return;
        }

        Instance.StartTransition(targetSceneName, showLoadingBar);
    }

    // Convenience wrappers for Unity Button OnClick() events
    public void LoadGameplay() => TransitionTo("Gameplay", showLoadingBar: true);
    public void LoadMainMenu() => TransitionTo("MainMenu", showLoadingBar: true);

    private void StartTransition(string targetSceneName, bool showLoadingBar)
    {
        if (isTransitioning) return;
        StartCoroutine(ExecuteTransition(targetSceneName, showLoadingBar));
    }

    private IEnumerator ExecuteTransition(string targetSceneName, bool showLoadingBar)
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;

        // Always reset pause state before switching scenes
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Scene previousScene = SceneManager.GetActiveScene();

        // 1. Close Shutters & Fade In (EaseOutCubic)
        yield return AnimateOverlay(0f, 1f, duration, easeOut: true);

        // 2. Show Loading Bar if requested
        if (showLoadingBar && loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (progressBar != null) progressBar.value = 0f;
            if (progressText != null) progressText.text = "LOADING... 0%";
        }

        // 3. Load Target Scene Asynchronously (hold activation until complete)
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        if (loadOp != null)
        {
            loadOp.allowSceneActivation = false;
            float elapsed = 0f;

            while (!loadOp.isDone)
            {
                elapsed += Time.unscaledDeltaTime;
                float realProgress = Mathf.Clamp01(loadOp.progress / 0.9f);
                float displayProgress = showLoadingBar
                    ? Mathf.Min(realProgress, Mathf.Clamp01(elapsed / minimumLoadTime))
                    : realProgress;

                if (showLoadingBar)
                {
                    if (progressBar != null) progressBar.value = displayProgress;
                    if (progressText != null) progressText.text = $"LOADING... {Mathf.RoundToInt(displayProgress * 100)}%";
                }

                // Player cannot enter gameplay until real async load hits 0.9 (ready)
                if (loadOp.progress >= 0.9f && (!showLoadingBar || displayProgress >= 0.99f))
                {
                    if (showLoadingBar)
                    {
                        if (progressBar != null) progressBar.value = 1f;
                        if (progressText != null) progressText.text = "LOADING... 100%";
                    }
                    loadOp.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        Scene loadedScene = SceneManager.GetSceneByName(targetSceneName);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
        }

        // 4. Unload Previous Scene (keep TransitionScene loaded)
        if (previousScene.IsValid() && previousScene.name != TransitionSceneName)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(previousScene);
            while (unloadOp != null && !unloadOp.isDone) yield return null;
        }

        // 5. Crossfade Music based on destination scene
        if (AudioManager.Instance != null)
        {
            if (targetSceneName == "Gameplay" && AudioManager.Instance.gameplayMusic != null)
                AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic, 1.0f);
            else if (targetSceneName == "MainMenu" && AudioManager.Instance.menuMusic != null)
                AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic, 1.0f);
        }

        if (loadingPanel != null) loadingPanel.SetActive(false);

        yield return new WaitForSecondsRealtime(0.08f);

        // 6. Open Shutters & Fade Out (EaseInCubic)
        yield return AnimateOverlay(1f, 0f, duration, easeOut: false);

        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    private IEnumerator AnimateOverlay(float startAlpha, float endAlpha, float time, bool easeOut)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            float eased = easeOut ? (1f - Mathf.Pow(1f - t, 3f)) : (t * t * t);
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, eased);

            if (topShutter != null && bottomShutter != null)
            {
                float offset = Mathf.Lerp(easeOut ? 560f : 0f, easeOut ? 0f : 560f, eased);
                topShutter.anchoredPosition = new Vector2(0f, offset);
                bottomShutter.anchoredPosition = new Vector2(0f, -offset);
            }

            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
    }
}