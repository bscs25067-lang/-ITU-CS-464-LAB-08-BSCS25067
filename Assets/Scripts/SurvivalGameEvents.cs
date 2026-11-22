using System;
using UnityEngine;


[DefaultExecutionOrder(-1)]
public class SurvivalGameEvents : MonoBehaviour
{
    public static SurvivalGameEvents Instance { get; private set; }

    [Header("Game Rules")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int targetItemsToWin = 5;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public int CollectedItems { get; private set; }
    public int TargetItems => targetItemsToWin;
    public bool IsGameOver { get; private set; }

    // C# Events (Publisher)
    public event Action<int, int> OnDamaged;          // (currentHealth, maxHealth)
    public event Action<int, int> OnHealed;           // (currentHealth, maxHealth)
    public event Action<int, int> OnItemCollected;    // (collected, target)
    public event Action OnDied;
    public event Action OnGameWon;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentHealth = maxHealth;
        CollectedItems = 0;
        IsGameOver = false;
    }

    public void TakeDamage(int amount)
    {
        if (IsGameOver) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnDamaged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0)
        {
            IsGameOver = true;
            OnDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (IsGameOver || CurrentHealth >= maxHealth) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealed?.Invoke(CurrentHealth, maxHealth);
    }

    public void CollectItem()
    {
        if (IsGameOver) return;

        CollectedItems++;
        OnItemCollected?.Invoke(CollectedItems, targetItemsToWin);

        if (CollectedItems >= targetItemsToWin)
        {
            IsGameOver = true;
            OnGameWon?.Invoke();
        }
    }
}