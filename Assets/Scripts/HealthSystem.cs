using System;
using UnityEngine;

// Single source of truth for health on anything that can be damaged (player, enemies, props).
// Other components react to damage/death through the events instead of tracking health themselves.
public class HealthSystem : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("Destroy the GameObject on death. If off, it is deactivated instead (useful for the player / respawning).")]
    [SerializeField] private bool destroyOnDeath = true;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<DamagePayload> Damaged;
    public event Action Died;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(DamagePayload payload)
    {
        if (IsDead) return;

        // Elemental modifiers (resistances, burn, etc.) will hook in here
        float finalDamage = payload.amount;

        CurrentHealth = Mathf.Max(CurrentHealth - finalDamage, 0f);
        Debug.Log($"{gameObject.name} took {finalDamage} {payload.type} damage. Health: {CurrentHealth}/{maxHealth}");

        Damaged?.Invoke(payload);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log($"{gameObject.name} was destroyed!");
        Died?.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
