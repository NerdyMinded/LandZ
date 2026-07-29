using UnityEngine;

public class EnemyRewardTarget : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Loot Drop")]
    public GameObject rewardPrefab;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamagePayload payload)
    {
        currentHealth -= payload.amount;
        Debug.Log($"{gameObject.name} took {payload.amount} {payload.type} damage! Remaining HP: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (rewardPrefab != null)
        {
            // Spawn loot slightly above enemy center
            GameObject loot = Instantiate(rewardPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            
            // Give the spawned loot a small upward/random pop impulse
            Rigidbody lootRb = loot.GetComponent<Rigidbody>();
            if (lootRb != null)
            {
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f)).normalized;
                lootRb.AddForce(randomDirection * 3f, ForceMode.Impulse);
            }
        }

        Debug.Log($"{gameObject.name} eliminated!");
        Destroy(gameObject);
    }
}