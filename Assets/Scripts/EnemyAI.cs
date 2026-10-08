using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection & Movement")]
    public float detectionRadius = 15.0f; // Distance at which enemy spots player
    public float moveSpeed = 3.5f;        // Enemy movement speed
    public float attackRange = 1.8f;      // Melee attack range
    public float attackDamage = 15.0f;    // Damage dealt to player
    public float attackCooldown = 1.5f;   // Seconds between attacks

    private Transform playerTransform;
    private NavMeshAgent agent;
    private float lastAttackTime;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
        }

        // Locate player directly via PlayerController
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Spot player if within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            // 1. Move toward player
            if (distanceToPlayer > attackRange)
            {
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(playerTransform.position);
                }
                else
                {
                    // Direct movement fallback if no NavMesh is set up
                    Vector3 direction = (playerTransform.position - transform.position).normalized;
                    direction.y = 0; // Stay flat on ground
                    if (direction != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
                    }
                    transform.position += transform.forward * moveSpeed * Time.deltaTime;
                }
            }
            else
            {
                // Stop moving when within attack range
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }

                // Face the player
                Vector3 lookDir = (playerTransform.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }

                // 2. Attack Player
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    AttackPlayer();
                }
            }
        }
    }

    private void AttackPlayer()
    {
        lastAttackTime = Time.time;

        IDamageable playerDamageable = playerTransform.GetComponentInParent<IDamageable>();
        if (playerDamageable != null)
        {
            DamagePayload payload = new DamagePayload
            {
                amount = attackDamage,
                type = DamageType.Physical,
                hitPoint = playerTransform.position,
                hitNormal = Vector3.up
            };

            playerDamageable.TakeDamage(payload);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizes Detection (Yellow) & Attack (Red) ranges in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}