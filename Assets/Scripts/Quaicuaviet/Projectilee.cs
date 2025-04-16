using UnityEngine;

public class Projectilee : MonoBehaviour
{
    private Rigidbody2D rb;
    public GameObject explosionEffect; // Prefab cho hiệu ứng nổ

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Projectile hit: " + other.name + " (Tag: " + other.tag + ")");

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                Debug.Log("Player hit by projectile! HP: " + playerHealth.currentHealth);
                // Tạo hiệu ứng nổ khi trúng Player
                if (explosionEffect != null)
                {
                    Instantiate(explosionEffect, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("PlayerHealth component not found on Player!");
            }
        }
        else if (other.CompareTag("Ground"))
        {
            // Tạo hiệu ứng nổ khi chạm Ground
            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject); // Hủy chưởng khi chạm Ground
        }
    }

    void Update()
    {
        if (Vector2.Distance(transform.position, Vector2.zero) > 20f)
        {
            Destroy(gameObject);
        }
    }
}