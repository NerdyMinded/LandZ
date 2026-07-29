using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamagePayload payload)
    {
        // Apply damage logic based on elemental type
        float finalDamage = payload.amount;

        // Example: Handle specific elemental modifiers
        switch (payload.type)
        {
            case DamageType.Fire:
                Debug.Log($"{gameObject.name} took {finalDamage} FIRE damage! (Burn effect queued)");
                break;
            case DamageType.Toxic:
                Debug.Log($"{gameObject.name} took {finalDamage} TOXIC damage!");
                break;
            default:
                Debug.Log($"{gameObject.name} took {finalDamage} physical damage.");
                break;
        }

        currentHealth -= finalDamage;
        Debug.Log($"{gameObject.name} Health Remaining: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} was destroyed!");
        gameObject.SetActive(false); // Can be replaced with ragdoll or destroy call
    }
}
