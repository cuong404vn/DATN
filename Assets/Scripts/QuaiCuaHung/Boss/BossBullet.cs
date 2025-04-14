using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 3f;
    public int damage = 1; // Giảm sát thương xuống 1 (sẽ sửa tiếp ở mục 3)
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
        // Xoay sprite đạn theo hướng bay (nếu cần)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate()
    {
        // Dùng Rigidbody2D để di chuyển đạn
        rb.linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}