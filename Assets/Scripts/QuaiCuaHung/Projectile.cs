using UnityEngine;

public class Projectile : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Projectile hit: " + other.name); // Debug để kiểm tra va chạm
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1); // Gây 1 sát thương
                Debug.Log("Player hit by projectile! HP: " + playerHealth.currentHealth);
                Destroy(gameObject); // Hủy chưởng ngay sau khi trúng Player
            }
            else
            {
                Debug.LogWarning("PlayerHealth component not found on Player!");
            }
        }
        // Không hủy chưởng khi va chạm với Vampire hoặc Ground
    }

    void Update()
    {
        // Hủy chưởng nếu bay quá xa
        if (Vector2.Distance(transform.position, Vector2.zero) > 20f)
        {
            Destroy(gameObject);
        }
    }
}