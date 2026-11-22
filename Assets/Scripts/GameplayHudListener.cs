using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayHUDListener : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text ecgStatusText;
    [SerializeField] private TMP_Text itemCountText;

    [Header("Results Panel")]
    [SerializeField] private GameObject resultsCanvas;
    [SerializeField] private TMP_Text resultsTitleText;

    private void OnEnable()
    {
        if (SurvivalGameEvents.Instance == null) return;

        SurvivalGameEvents.Instance.OnDamaged += HandleHealthChanged;
        SurvivalGameEvents.Instance.OnHealed += HandleHealthChanged;
        SurvivalGameEvents.Instance.OnItemCollected += HandleItemCollected;
        SurvivalGameEvents.Instance.OnDied += HandleDied;
        SurvivalGameEvents.Instance.OnGameWon += HandleGameWon;
    }

    private void OnDisable()
    {
        if (SurvivalGameEvents.Instance == null) return;
        SurvivalGameEvents.Instance.OnDamaged -= HandleHealthChanged;
        SurvivalGameEvents.Instance.OnHealed -= HandleHealthChanged;
        SurvivalGameEvents.Instance.OnItemCollected -= HandleItemCollected;
        SurvivalGameEvents.Instance.OnDied -= HandleDied;
        SurvivalGameEvents.Instance.OnGameWon -= HandleGameWon;
    }

    private void Start()
    {
        if (resultsCanvas) resultsCanvas.SetActive(false);
        if (SurvivalGameEvents.Instance != null)
        {
            HandleHealthChanged(SurvivalGameEvents.Instance.CurrentHealth, SurvivalGameEvents.Instance.MaxHealth);
            HandleItemCollected(0, SurvivalGameEvents.Instance.TargetItems);
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        float ratio = (float)current / max;
        if (healthBar) healthBar.value = ratio;

        if (ecgStatusText)
        {
            if (ratio > 0.6f) { ecgStatusText.text = "FINE"; ecgStatusText.color = Color.green; }
            else if (ratio > 0.25f) { ecgStatusText.text = "CAUTION"; ecgStatusText.color = Color.yellow; }
            else { ecgStatusText.text = "DANGER"; ecgStatusText.color = Color.red; }
        }
    }

    private void HandleItemCollected(int collected, int target)
    {
        if (itemCountText) itemCountText.text = $"KEY ITEMS: {collected} / {target}";
    }

    private void HandleDied()
    {
        ShowResults("YOU ARE DEAD");
    }

    private void HandleGameWon()
    {
        ShowResults("SCENARIO CLEARED");
    }

    private void ShowResults(string title)
    {
        Time.timeScale = 0f;
        if (resultsTitleText) resultsTitleText.text = title;
        if (resultsCanvas) resultsCanvas.SetActive(true);
    }
}