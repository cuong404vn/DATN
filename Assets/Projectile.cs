using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    private int direction = 1; // Hướng mặc định (1 = phải, -1 = trái)

    public void SetDirection(bool isFacingRight)
    {
        direction = isFacingRight ? 1 : -1;

        // Xoay viên đạn theo cùng chiều quái
        transform.localScale = new Vector3(direction, 1, 1);
    }

    void Update()
    {
        // Di chuyển viên đạn theo trục X
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ground"))
        {
            Destroy(gameObject); // Hủy đạn khi chạm vào Player hoặc nền đất
        }
    }
}
