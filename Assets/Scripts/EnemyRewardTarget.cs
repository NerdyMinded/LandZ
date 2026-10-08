using UnityEngine;

// Drops a reward when the attached HealthSystem dies. Health itself lives on HealthSystem.
[RequireComponent(typeof(HealthSystem))]
public class EnemyRewardTarget : MonoBehaviour
{
    [Header("Reward Settings")]
    public GameObject rewardPrefab;

    private HealthSystem health;

    private void Awake()
    {
        health = GetComponent<HealthSystem>();
    }

    private void OnEnable()
    {
        health.Died += DropReward;
    }

    private void OnDisable()
    {
        health.Died -= DropReward;
    }

    private void DropReward()
    {
        if (rewardPrefab == null) return;

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
}
