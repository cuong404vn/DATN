using UnityEngine;

public class EnemyPatroll : MonoBehaviour
{
    public float patrolDistance = 3f;
    public float moveSpeed = 2f;

    private Vector2 initialPosition;
    private int direction = 1;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        // Di chuyển
        transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);

        // Kiểm tra khoảng cách đã đi
        if (Mathf.Abs(transform.position.x - initialPosition.x) >= patrolDistance)
        {
            // Đổi hướng
            direction *= -1;

            // Quay mặt quái
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}
