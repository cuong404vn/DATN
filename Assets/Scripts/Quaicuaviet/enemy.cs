using UnityEngine;

public class enemy : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float moveDistance = 2f;
    public float detectionRange = 2f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public float attackDamage = 10f;

    private Vector3 startPosition;
    private bool movingRight = true;
    private Transform player;
    private bool isPlayerInRange = false;
    private float lastAttackTime;
    private Rigidbody2D rb;
    private Animator anim;
    private bool wasWalkingBeforeAttack;

    public AudioClip attackSound;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Ensure the Player has the 'Player' tag.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Debug.Log("Distance to Player: " + distanceToPlayer);

        if (distanceToPlayer <= detectionRange)
        {
            isPlayerInRange = true;
        }
        else
        {
            isPlayerInRange = false;
        }

        if (isPlayerInRange)
        {
            if (distanceToPlayer > attackRange)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
                anim.SetBool("isWalking", true);
                anim.ResetTrigger("attack");
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                anim.SetBool("isWalking", false);

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    wasWalkingBeforeAttack = anim.GetBool("isWalking");
                    Attack();
                    lastAttackTime = Time.time;
                }
            }

            if (player.position.x < transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1);
            else
                transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            Patrol();
            anim.ResetTrigger("attack");
        }
    }

    void Patrol()
    {
        if (movingRight)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(-1, 1, 1);
        }

        if (transform.position.x > startPosition.x + moveDistance)
            movingRight = false;
        else if (transform.position.x < startPosition.x - moveDistance)
            movingRight = true;

        anim.SetBool("isWalking", true);
    }

    void Attack()
    {
        Debug.Log("Attack triggered!");
        anim.SetTrigger("attack");

        if (attackSound != null && audioSource != null)
        {
            Debug.Log("Playing attack sound for " + gameObject.name);
            audioSource.PlayOneShot(attackSound);
        }
        else
        {
            Debug.LogWarning("Attack sound or AudioSource is missing on " + gameObject.name);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        Debug.Log("Checking for hits in range: " + attackRange + ", found: " + hits.Length);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Player hit! Damage: " + attackDamage);
                hit.GetComponent<PlayerHealth>().TakeDamage((int)attackDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}