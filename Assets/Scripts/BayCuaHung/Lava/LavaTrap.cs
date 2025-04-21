using UnityEngine;

public class LavaTrap : MonoBehaviour
{
    public GameObject lavaDropPrefab; // Prefab giọt dung nham
    public float spawnInterval = 2f; // Thời gian giữa các lần spawn

    void Start()
    {
        InvokeRepeating("SpawnLavaDrop", spawnInterval, spawnInterval);
    }

    void SpawnLavaDrop()
    {
        Instantiate(lavaDropPrefab, transform.position, Quaternion.identity);
    }
}