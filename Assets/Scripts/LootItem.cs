using UnityEngine;

public class LootItem : MonoBehaviour
{
    [Header("Item Data")]
    public string itemName = "Scrap Metal";
    public int amount = 10;

    [Header("Magnet / Collection Settings")]
    public float magnetRadius = 6.0f;    // Range at which it starts flying to you
    public float flySpeed = 15.0f;       // Speed of magnet pull
    public float collectDistance = 1.2f; // Range at which it gets collected

    private Transform playerTransform;
    private Rigidbody rb;
    private bool isMagnetized; // Once pulled, keep flying to the player even if they move out of range

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Finds PlayerController component directly (works even if Tag is Untagged!)
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (!isMagnetized && distance <= magnetRadius)
        {
            isMagnetized = true;
            if (rb != null)
            {
                rb.isKinematic = true; // Disable physics forces while pulling
            }
        }

        // Pull item toward player
        if (isMagnetized)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                playerTransform.position + Vector3.up * 1.0f,
                flySpeed * Time.deltaTime
            );

            if (distance <= collectDistance)
            {
                Collect();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null || other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (!enabled) return; // Guard against collecting twice in one frame (trigger + distance)
        enabled = false;

        Debug.Log($"[REWARD COLLECTED] {amount} {itemName}!");
        Destroy(gameObject);
    }
}