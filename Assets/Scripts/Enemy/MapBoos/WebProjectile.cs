using UnityEngine;

public class WebProjectile : MonoBehaviour
{
    public float speed = 7f;
    public int damage = 1;
    public GameObject sourceNhen;

    private Rigidbody2D rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0.5f;
        }


        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.2f;
            collider.isTrigger = true;
        }

    }

    public void Initialize(float direction, float projectileSpeed, int damageAmount, GameObject source)
    {

        damage = damageAmount;
        speed = projectileSpeed;
        sourceNhen = source;



        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * speed, 0);
        }


        if (source != null)
        {
            Collider2D sourceCollider = source.GetComponent<Collider2D>();
            Collider2D webCollider = GetComponent<Collider2D>();

            if (sourceCollider != null && webCollider != null)
            {
                Physics2D.IgnoreCollision(webCollider, sourceCollider, true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (hasHit)
        {
            return;
        }


        if (sourceNhen != null && collision.gameObject == sourceNhen)
        {
            return;
        }


        if (collision.CompareTag("Player"))
        {
            hasHit = true;

            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            else
            {
            }


            Destroy(gameObject);
        }

        else if (collision.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        Destroy(gameObject, 5f);
    }
}