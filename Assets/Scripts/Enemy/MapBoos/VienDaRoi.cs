using UnityEngine;

public class VienDaRoi : MonoBehaviour
{
    public int damage = 15;
    public float fallSpeed = 10f;
    public float destroyOnImpactDelay = 0.5f;
    public GameObject impactEffectPrefab;
    public AudioClip impactSound;

    private Rigidbody2D rb;
    private bool hasHitSomething = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }


        rb.gravityScale = 2f;
        rb.mass = 2f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {

        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));


        rb.AddForce(new Vector2(Random.Range(-0.5f, 0.5f), 0) * 2f, ForceMode2D.Impulse);


        Destroy(gameObject, 5f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHitSomething) return;
        hasHitSomething = true;


        if (collision.gameObject.CompareTag("Player"))
        {

            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }


        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }


        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position);
        }


        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;


        Collider2D rockCollider = GetComponent<Collider2D>();
        if (rockCollider != null)
        {
            rockCollider.enabled = false;
        }


        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeOut(spriteRenderer));
        }


        Destroy(gameObject, destroyOnImpactDelay);
    }


    System.Collections.IEnumerator FadeOut(SpriteRenderer renderer)
    {
        Color originalColor = renderer.color;
        float elapsed = 0;

        while (elapsed < destroyOnImpactDelay)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / destroyOnImpactDelay);
            renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    }
}