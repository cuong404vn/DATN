using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public float patrolDistance = 3f;
    private Vector2 startPos;
    private bool movingRight = true;

    public Transform player;
    public float detectionRange = 5f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;
    public float fireRate = 1f;
    private float nextFireTime;

    public int health = 1;

    private Vector3 initialScale;

    void Start()
    {
        startPos = transform.position;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        initialScale = transform.localScale;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            FacePlayer();
            Shoot();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        Vector2 targetPos = movingRight ?
            new Vector2(startPos.x + patrolDistance, transform.position.y) :
            new Vector2(startPos.x - patrolDistance, transform.position.y);

        transform.position = Vector2.MoveTowards(transform.position, targetPos, patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            movingRight = !movingRight;
        }

        transform.localScale = new Vector3(
            movingRight ? Mathf.Abs(initialScale.x) : -Mathf.Abs(initialScale.x),
            initialScale.y,
            initialScale.z
        );
    }

    void FacePlayer()
    {
        transform.localScale = new Vector3(
            player.position.x < transform.position.x ? -Mathf.Abs(initialScale.x) : Mathf.Abs(initialScale.x),
            initialScale.y,
            initialScale.z
        );
    }

    void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            Vector2 direction = player.position.x < transform.position.x ? Vector2.left : Vector2.right;
            // Tăng offset để chưởng không va chạm ngay với Vampire
            Vector2 spawnPosition = (Vector2)transform.position + direction * 1f; // Offset 1 đơn vị
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileSpeed;
            projectile.transform.localScale = new Vector3(direction.x >= 0 ? 1 : -1, 1, 1);

            nextFireTime = Time.time + fireRate;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                Debug.Log("Player hit by collision! HP: " + playerHealth.currentHealth);
            }
            else
            {
                Debug.LogWarning("PlayerHealth component not found on Player during collision!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy HP: " + health);
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}