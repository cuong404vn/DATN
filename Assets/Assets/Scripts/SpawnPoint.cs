using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); 
        if (player != null)
        {
            player.transform.position = transform.position; 
        }
    }
}
