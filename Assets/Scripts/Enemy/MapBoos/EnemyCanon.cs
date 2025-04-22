using UnityEngine;
using System.Collections;

public class EnemyCanon : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireRate = 3f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float arcHeight = 3f;
    [SerializeField] private float targetOffsetDistance = 2f;
    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip explosionSound;

    private Transform player;
    private AudioSource audioSource;
    private bool canFire = true;
    private readonly int hashFire = Animator.StringToHash("Fire");
    private float lastFireTime = 0f;
    private bool facingRight = true;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Start()
    {

        if (firePoint == null)
        {
            firePoint = transform;
        }


        if (projectilePrefab != null && projectilePrefab.GetComponent<CanonProjectile>() == null)
        {
            Debug.LogWarning("ProjectilePrefab không có component CanonProjectile. Tạo mới khi bắn.");
        }
    }

    private void Update()
    {
        if (player == null) return;


        float distanceToPlayer = Vector2.Distance(transform.position, player.position);


        UpdateFacingDirection();


        if (distanceToPlayer <= detectionRange && canFire)
        {
            StartCoroutine(FireProjectile());
        }
    }

    private void UpdateFacingDirection()
    {
        if (player == null) return;


        if (player.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {

        facingRight = !facingRight;


        transform.Rotate(0f, 180f, 0f);

        Debug.Log($"Canon flipped, now facing {(facingRight ? "right" : "left")}");
    }

    private IEnumerator FireProjectile()
    {
        canFire = false;


        while (Time.time < lastFireTime + fireRate)
        {
            yield return null;
        }


        lastFireTime = Time.time;


        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }


        if (animator != null)
        {
            animator.SetTrigger("Fire");
        }


        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 playerPosition = player.position;


        Vector2 targetPosition = playerPosition - directionToPlayer * targetOffsetDistance;


        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);


        LaunchProjectile(projectile, targetPosition);


        yield return new WaitForSeconds(fireRate);

        canFire = true;
    }



    private void LaunchProjectile(GameObject projectile, Vector2 targetPosition)
    {

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody2D>();
        }


        rb.gravityScale = 1.0f;
        rb.mass = 1.0f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;


        CanonProjectile projectileScript = projectile.GetComponent<CanonProjectile>();
        if (projectileScript == null)
        {
            projectileScript = projectile.AddComponent<CanonProjectile>();
            projectileScript.explosionRadius = explosionRadius;
            projectileScript.damage = damageAmount;
            projectileScript.explosionEffectPrefab = explosionEffectPrefab;
            projectileScript.explosionSound = explosionSound;
            projectileScript.damageLayers = playerLayer;
        }


        CircleCollider2D collider = projectile.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = projectile.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
        }


        Vector2 startPos = firePoint.position;


        float displacementX = targetPosition.x - startPos.x;
        float displacementY = targetPosition.y - startPos.y;
        bool isLeftShot = displacementX < 0;


        Debug.Log($"Fire data - startPos: {startPos}, targetPos: {targetPosition}, displacementX: {displacementX}, displacementY: {displacementY}, isLeftShot: {isLeftShot}");


        float absDisplacementX = Mathf.Abs(displacementX);


        float baseAngleInDegrees = 45.0f;
        float gravity = Physics2D.gravity.magnitude * rb.gravityScale;


        float adjustedAngleInDegrees = baseAngleInDegrees;
        if (displacementY > 0)
        {

            adjustedAngleInDegrees = Mathf.Clamp(baseAngleInDegrees + displacementY * 2.0f, 45.0f, 75.0f);
        }


        float angleInRadians = adjustedAngleInDegrees * Mathf.Deg2Rad;


        Debug.Log($"Angle - base: {baseAngleInDegrees}, adjusted: {adjustedAngleInDegrees}, radians: {angleInRadians}");


        float distanceToTarget = Vector2.Distance(startPos, targetPosition);
        float initialVelocity;


        if (distanceToTarget < 1.0f)
        {
            Debug.Log("Target too close, using default velocity");
            initialVelocity = projectileSpeed;
        }
        else
        {
            try
            {

                float cos = Mathf.Cos(angleInRadians);
                float sin = Mathf.Sin(angleInRadians);
                float tan = Mathf.Tan(angleInRadians);



                float denominator = 2.0f * cos * cos * (displacementY + absDisplacementX * tan);

                Debug.Log($"Physics params - cos: {cos}, sin: {sin}, tan: {tan}, denominator: {denominator}");


                if (Mathf.Abs(denominator) < 0.001f || float.IsNaN(denominator) || float.IsInfinity(denominator))
                {
                    Debug.LogWarning("Invalid denominator in trajectory calculation. Using default velocity.");
                    initialVelocity = projectileSpeed;
                }
                else
                {

                    float numerator = gravity * absDisplacementX * absDisplacementX;


                    if (numerator < 0 || float.IsNaN(numerator) || float.IsInfinity(numerator))
                    {
                        Debug.LogWarning("Invalid numerator in trajectory calculation. Using default velocity.");
                        initialVelocity = projectileSpeed;
                    }
                    else
                    {

                        initialVelocity = Mathf.Sqrt(numerator / denominator);


                        if (float.IsNaN(initialVelocity) || float.IsInfinity(initialVelocity))
                        {
                            Debug.LogWarning("Invalid velocity calculated. Using default velocity.");
                            initialVelocity = projectileSpeed;
                        }

                        Debug.Log($"Velocity calculation - numerator: {numerator}, denominator: {denominator}, result: {initialVelocity}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error in trajectory calculation: " + e.Message);
                initialVelocity = projectileSpeed;
            }
        }



        initialVelocity = Mathf.Clamp(initialVelocity, projectileSpeed * 0.5f, projectileSpeed * 2.0f);


        Vector2 direction;
        if (isLeftShot)
        {

            direction = new Vector2(-Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        }
        else
        {

            direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        }


        if (float.IsNaN(direction.x) || float.IsNaN(direction.y) ||
            float.IsInfinity(direction.x) || float.IsInfinity(direction.y))
        {
            Debug.LogWarning("Invalid direction vector. Using default direction.");
            direction = isLeftShot ? Vector2.left : Vector2.right;
            direction.y = 0.5f;
        }


        rb.linearVelocity = direction * initialVelocity;


        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (float.IsNaN(angle) || float.IsInfinity(angle))
        {
            angle = isLeftShot ? 150f : 30f;
        }
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);


        Debug.Log($"Final launch params - velocity: {initialVelocity}, direction: {direction}, resultSpeed: {rb.linearVelocity}, angle: {angle}");


        StartCoroutine(UpdateProjectileRotation(projectile));
    }


    private IEnumerator UpdateProjectileRotation(GameObject projectile)
    {
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb == null) yield break;


        while (projectile != null && rb != null)
        {

            if (rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
            }


            CanonProjectile projectileScript = projectile.GetComponent<CanonProjectile>();
            if (projectileScript != null && projectileScript.IsRolling())
            {

                break;
            }

            yield return null;
        }
    }



    private void ExplodeProjectile(GameObject projectile)
    {
        if (projectile == null) return;


        CanonProjectile projectileScript = projectile.GetComponent<CanonProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Explode();
            return;
        }


        Vector2 explosionPos = projectile.transform.position;


        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, explosionPos, Quaternion.identity);
        }


        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }


        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionPos, explosionRadius, playerLayer);
        foreach (Collider2D hitCollider in hitColliders)
        {
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }


        Destroy(projectile);
    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);


        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
