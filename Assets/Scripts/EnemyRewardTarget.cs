using UnityEngine;

public class EnemyRewardTarget : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Reward Settings")]
    public GameObject rewardPrefab;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamagePayload payload)
    {
        currentHealth -= payload.amount;
        Debug.Log($"[ENEMY] Took {payload.amount} damage ({payload.type}). Health remaining: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (rewardPrefab != null)
        {
            GameObject reward = Instantiate(rewardPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Rigidbody rewardRb = reward.GetComponent<Rigidbody>();
            if (rewardRb != null)
            {
                // Upward pop with minimal horizontal scatter
                Vector3 spawnForce = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    3.0f,
                    Random.Range(-0.5f, 0.5f)
                );
                rewardRb.AddForce(spawnForce, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}