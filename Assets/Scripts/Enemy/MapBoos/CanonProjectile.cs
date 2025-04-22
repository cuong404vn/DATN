using UnityEngine;
using System.Collections;

public class CanonProjectile : MonoBehaviour
{
    [Header("Properties")]
    public float explosionRadius = 2f;
    public int damage = 10;
    public GameObject explosionEffectPrefab;
    public AudioClip explosionSound;
    public LayerMask damageLayers;
    public float explosionEffectDuration = 1f;

    [Header("Rolling")]
    public float rollingSpeed = 2f;
    public float maxRollingTime = 2f;

    private bool hasExploded = false;
    private bool isRolling = false;
    private Rigidbody2D rb;

    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }


        rb.gravityScale = 0;
        rb.mass = 1f;
        rb.linearDamping = 0.5f;
        rb.constraints = RigidbodyConstraints2D.None;
    }


    public void StartRolling()
    {
        if (hasExploded) return;

        isRolling = true;


        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
        }


        rb.gravityScale = 1.0f;
        rb.mass = 1.0f;
        rb.linearDamping = 0.3f;
        rb.angularDamping = 0.1f;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;


        Vector2 rollingDirection = new Vector2(Mathf.Sign(rb.linearVelocity.x), 0);
        if (rollingDirection.x == 0) rollingDirection.x = transform.right.x;
        rb.AddForce(rollingDirection * rollingSpeed, ForceMode2D.Impulse);


        StartCoroutine(ExplodeAfterDelay(maxRollingTime));


        Debug.Log("Canon projectile started rolling with velocity: " + rb.linearVelocity);
    }

    private void Update()
    {

        if (isRolling && rb != null && !hasExploded)
        {

            rb.angularVelocity = rb.linearVelocity.x * 360f;


            if (Mathf.Abs(rb.linearVelocity.x) < 0.2f)
            {

                Vector2 rollingDirection = new Vector2(Mathf.Sign(rb.linearVelocity.x), 0);
                if (rollingDirection.x == 0) rollingDirection.x = transform.right.x;
                rb.AddForce(rollingDirection * rollingSpeed * 0.2f, ForceMode2D.Force);
            }


            if (Mathf.Abs(rb.linearVelocity.x) > rollingSpeed)
            {
                rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * rollingSpeed, rb.linearVelocity.y);
            }
        }
    }

    private IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {

            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Canon projectile hit Player directly with damage {damage}.");
            }

            Explode();
        }

        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {

            if (!isRolling)
            {
                StartRolling();


                Vector2 contactNormal = collision.contacts[0].normal;
                Vector2 rollingDirection = new Vector2(contactNormal.y, -contactNormal.x).normalized;


                if (Vector2.Dot(rollingDirection, Vector2.right) < 0)
                {
                    rollingDirection = -rollingDirection;
                }


                rb.linearVelocity = rollingDirection * rollingSpeed;


                rb.angularVelocity = rollingDirection.x * 360f;

                Debug.Log("Canon projectile hit ground and started rolling with direction: " + rollingDirection);
            }

            else
            {

                rb.constraints = RigidbodyConstraints2D.FreezePositionY;


                if (Mathf.Abs(rb.linearVelocity.x) < 0.5f)
                {
                    Vector2 rollingDirection = new Vector2(Mathf.Sign(rb.linearVelocity.x), 0);
                    if (rollingDirection.x == 0) rollingDirection.x = transform.right.x;
                    rb.AddForce(rollingDirection * rollingSpeed * 0.5f, ForceMode2D.Impulse);
                }
            }
        }
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;


        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }


        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);


            Destroy(explosionEffect, explosionEffectDuration);
        }


        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }


        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageLayers);
        foreach (Collider2D hitCollider in hitColliders)
        {

            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log($"Canon projectile explosion hit Player with damage {damage}.");
                playerHealth.TakeDamage(damage);
            }
        }


        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }


        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }


        Destroy(gameObject, explosionEffectDuration);
    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }


    public bool IsRolling()
    {
        return isRolling;
    }
}
