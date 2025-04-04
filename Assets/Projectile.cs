using UnityEngine;

public class Projectile : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 5f); // Hủy sau 5 giây
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Trúng Player!");
            Destroy(gameObject); // Hủy khi trúng Player
        }
    }
}