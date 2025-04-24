using UnityEngine;

public class DripstoneTrap : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private GameObject triggerZone;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private int damage = 10;
    private bool isTriggered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void ActivateTrap()
    {
        if (!isTriggered)
        {
            isTriggered = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = new Vector2(0, -fallSpeed);
            Debug.Log("Dripstone is falling!");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Dripstone collided with: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Dealt {damage} damage to Player");
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Dripstone hit Ground");
            Destroy(gameObject);
        }
    }
}