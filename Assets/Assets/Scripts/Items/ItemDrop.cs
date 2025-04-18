using UnityEngine;
using System.Collections.Generic;

public class ItemDrop : MonoBehaviour
{
    [System.Serializable]
    public class ItemDropData
    {
        public GameObject itemPrefab;
        public float dropChance;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    public List<ItemDropData> possibleItems = new List<ItemDropData>();
    public float dropRadius = 1f;
    public Transform customDropPoint; 

    public void DropItem()
    {
       
        Vector3 basePosition = customDropPoint != null ?
            customDropPoint.position : transform.position;

        foreach (ItemDropData item in possibleItems)
        {
            if (Random.Range(0f, 100f) <= item.dropChance)
            {
                int amount = Random.Range(item.minAmount, item.maxAmount + 1);

                for (int i = 0; i < amount; i++)
                {
                    
                    Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
                    Vector3 spawnPosition = basePosition + new Vector3(randomOffset.x, randomOffset.y, 0);

                    Instantiate(item.itemPrefab, spawnPosition, Quaternion.identity);
                }
            }
        }
    }
}