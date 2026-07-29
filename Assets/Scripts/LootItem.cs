using UnityEngine;

public class LootItem : MonoBehaviour
{
    public string itemName = "Scrap Metal";
    public int amount = 10;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object colliding with the loot is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"[REWARD COLLECTED] +{amount} {itemName}!");
            Destroy(gameObject);
        }
    }
}