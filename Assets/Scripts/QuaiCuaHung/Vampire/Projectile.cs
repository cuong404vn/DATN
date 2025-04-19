using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1; // Sát thương của chưởng
    public GameObject explosionPrefab; // Prefab của hiệu ứng nổ

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Chưởng va chạm với: {collision.gameObject.name} (Tag: {collision.tag})");

        if (collision.CompareTag("Player") || collision.CompareTag("Ground"))
        {
            // Tạo hiệu ứng nổ
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }

            // Gây sát thương cho Player
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

            // Hủy chưởng
            Destroy(gameObject);
        }
    }
}