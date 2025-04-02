using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Di chuyển")]
    public float speed = 2f;
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    private Rigidbody2D rb;
    private bool movingRight = true;
    private bool isPatrolling = true;

    [Header("Phát hiện người chơi")]
    public float detectionRange = 5f;
    private Transform player;

    [Header("Bắn chưởng")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float nextFireTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < detectionRange)
        {
            isPatrolling = false; // Ngừng đi tuần khi phát hiện player
            rb.linearVelocity = Vector2.zero;

            // Xoay mặt quái theo hướng player
            FacePlayer();

            Shoot();
        }
        else
        {
            isPatrolling = true; 
        }

        if (isPatrolling)
        {
            Patrol();
        }
    }

    void FacePlayer()
{
    // Kiểm tra hướng của player
    if (player.position.x > transform.position.x && !movingRight)
    {
        Flip();
    }
    else if (player.position.x < transform.position.x && movingRight)
    {
        Flip();
    }
}

    void Patrol()
    {
        // Kiểm tra mép đường và tường
        bool isNearEdge = !Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        bool isWallAhead = Physics2D.OverlapCircle(wallCheck.position, 0.2f, groundLayer);

        // Nếu gặp mép đường hoặc tường, quay đầu
        if (isNearEdge || isWallAhead)
        {
            Flip();
        }

        // Di chuyển quái
        rb.linearVelocity = new Vector2(movingRight ? speed : -speed, rb.linearVelocity.y);
    }

    void Flip()
    {
        movingRight = !movingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            projectile.GetComponent<Projectile>().SetDirection(movingRight); // Truyền hướng quái vào viên đạn
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
}
