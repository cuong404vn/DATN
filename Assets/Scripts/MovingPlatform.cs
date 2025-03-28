using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;
    public float distance = 3f;
    private Vector3 startPosition;
    private float direction = 1f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Di chuyển platform qua lại
        transform.Translate(Vector2.right * speed * direction * Time.deltaTime);

        // Đổi hướng khi đến giới hạn
        if (Vector3.Distance(startPosition, transform.position) >= distance)
        {
            direction *= -1;
        }
    }

    // Đảm bảo player di chuyển cùng platform
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}