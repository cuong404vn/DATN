using UnityEngine;

public class Drop : MonoBehaviour
{
    public Sprite splashSprite; 
    public int damage = 10; 
    public float lifetime = 5f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); 
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            
            ShowSplashEffect();
        }
        else if (collision.CompareTag("Ground"))
        {
            
            ShowSplashEffect();
        }
    }

    void ShowSplashEffect()
    {
        spriteRenderer.sprite = splashSprite;
        rb.linearVelocity = Vector2.zero; 
        rb.gravityScale = 0; 
        Destroy(gameObject, 0.5f); 
    }
}
