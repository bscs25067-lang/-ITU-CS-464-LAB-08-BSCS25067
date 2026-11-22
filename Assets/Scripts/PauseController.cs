using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Pause UI References")]
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject audioSettingsPanel;

    private bool isPaused = false;

    private void Start()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (SurvivalGameEvents.Instance != null && SurvivalGameEvents.Instance.IsGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        Time.timeScale = 0f;
        AudioListener.pause = true; // Gameplay SFX pause; UI & Music continue via ignoreListenerPause

        if (pauseCanvas != null) pauseCanvas.SetActive(true);
        if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);

        AudioManager.Instance?.PlayPause();
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
        if (pauseCanvas != null) pauseCanvas.SetActive(false);

        AudioManager.Instance?.PlayPause();
    }

    public void ToggleAudioSettings()
    {
        if (audioSettingsPanel == null) return;
        bool nextState = !audioSettingsPanel.activeSelf;
        audioSettingsPanel.SetActive(nextState);
        AudioManager.Instance?.PlayPanelOpen();
    }

    public void ReplayLevel()
    {
        ResetPauseState();
        SceneTransition.TransitionTo(SceneManager.GetActiveScene().name, showLoadingBar: true);
    }

    public void ReturnToMainMenu()
    {
        ResetPauseState();
        SceneTransition.TransitionTo("02_MainMenu", showLoadingBar: true);
    }

    private void ResetPauseState()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void OnDestroy()
    {
        // Safety net in case the scene unloads while paused
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}