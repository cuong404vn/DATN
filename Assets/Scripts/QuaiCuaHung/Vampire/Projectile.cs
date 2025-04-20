using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1; 
    public GameObject explosionPrefab; 

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Chưởng va chạm với: {collision.gameObject.name} (Tag: {collision.tag})");

        if (collision.CompareTag("Player") || collision.CompareTag("Ground"))
        {
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }

            if (collision.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    Debug.Log($"Gây {damage} sát thương cho Player!");
                    playerHealth.TakeDamage(damage);
                }
                else
                {
                    Debug.LogError("Không tìm thấy PlayerHealth trên Player!");
                }
            }

            Destroy(gameObject);
        }
    }
}