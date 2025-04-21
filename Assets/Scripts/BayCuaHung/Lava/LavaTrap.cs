using UnityEngine;

public class LavaTrap : MonoBehaviour
{
    public GameObject lavaDropPrefab; 
    public float spawnInterval = 2f; 

    void Start()
    {
        InvokeRepeating("SpawnLavaDrop", spawnInterval, spawnInterval);
    }

    void SpawnLavaDrop()
    {
        Instantiate(lavaDropPrefab, transform.position, Quaternion.identity);
    }
}