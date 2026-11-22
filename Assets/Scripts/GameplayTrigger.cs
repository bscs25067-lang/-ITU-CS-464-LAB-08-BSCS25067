using UnityEngine;

public class GameplayTrigger : MonoBehaviour
{
    public enum TriggerType { KeyItem, Heal, Damage }

    [SerializeField] private TriggerType type;
    [SerializeField] private int amount = 30;
    [SerializeField] private bool destroyOnTrigger = true;
    [SerializeField] private float damageCooldown = 1f;

    private float nextTriggerTime;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || SurvivalGameEvents.Instance == null) return;
        if (Time.time < nextTriggerTime) return;

        switch (type)
        {
            case TriggerType.KeyItem:
                SurvivalGameEvents.Instance.CollectItem();
                break;
            case TriggerType.Heal:
                SurvivalGameEvents.Instance.Heal(amount);
                break;
            case TriggerType.Damage:
                SurvivalGameEvents.Instance.TakeDamage(amount);
                nextTriggerTime = Time.time + damageCooldown;
                break;
        }

        if (destroyOnTrigger && type != TriggerType.Damage)
        {
            Destroy(gameObject);
        }
    }
}