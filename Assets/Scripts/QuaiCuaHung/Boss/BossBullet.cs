using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 3f;
    public int damage = 1;
    public GameObject explosionEffectPrefab; // Prefab hiệu ứng nổ (dùng sprite)
    private Vector2 direction;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra va chạm với Player hoặc Ground
        if (other.CompareTag("Player") || other.CompareTag("Ground"))
        {
            // Tạo hiệu ứng nổ
            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Explosion Effect Prefab is not assigned in BossBullet!");
            }

            // Gây sát thương nếu va chạm với Player
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerHealth>().TakeDamage(damage);
            }

            // Hủy đạn
            Destroy(gameObject);
        }
    }
}