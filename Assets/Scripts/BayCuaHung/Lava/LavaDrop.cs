using UnityEngine;

public class LavaDrop : MonoBehaviour
{
    public Sprite splashSprite; // Sprite bắn tung tóe (Hình 3)
    public int damage = 10; // Sát thương gây ra
    public float lifetime = 5f; // Thời gian tồn tại của giọt

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); // Xóa giọt sau `lifetime` giây
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Gây sát thương cho người chơi
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            // Chuyển sang trạng thái bắn tung tóe
            ShowSplashEffect();
        }
        else if (collision.CompareTag("Ground"))
        {
            // Chạm đất, chuyển trạng thái bắn tung tóe
            ShowSplashEffect();
        }
    }

    void ShowSplashEffect()
    {
        spriteRenderer.sprite = splashSprite; // Đổi sprite sang Hình 3
        rb.linearVelocity = Vector2.zero; // Dừng chuyển động
        rb.gravityScale = 0; // Tắt trọng lực
        Destroy(gameObject, 0.5f); // Xóa sau 0.5 giây
    }
}