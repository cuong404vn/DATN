using UnityEngine;
using System.Collections;

public class EnemyQuaiRiu : MonoBehaviour
{

    public enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Die }
    [Header("State Machine")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrol;


    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D enemyCollider;



    [Header("Movement & Detection")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float detectionRangeX = 6f;
    [SerializeField] private float allowedYDifference = 1.5f;



    [SerializeField] private float patrolRange = 4f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.1f;


    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private AudioClip attackSound;



    private Transform player;

    private float lastAttackTime = -Mathf.Infinity;
    private bool isHurt = false;
    private bool isDead = false;
    private bool facingRight = true;
    private AudioSource audioSource;

    private Vector3 patrolCenter;
    private Vector3 currentPatrolTarget;
    private bool patrollingRight = true;


    private readonly int hashIsWalking = Animator.StringToHash("IsWalking");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashDie = Animator.StringToHash("Die");

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    void Awake()
    {

        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (enemyCollider == null) enemyCollider = GetComponent<Collider2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();


        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {

            enabled = false;
            return;
        }


        patrolCenter = transform.position;
        currentHealth = maxHealth;
    }

    void Start()
    {

        SetPatrolTarget(true);
    }

    void Update()
    {
        if (isDead || player == null || isHurt)
        {
            return;
        }


        float distanceToPlayer = Vector2.Distance(transform.position, player.position);


        UpdateCurrentState(distanceToPlayer);

        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
        }

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isDead || isHurt)
        {
            if (rb.linearVelocity != Vector2.zero)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void UpdateCurrentState(float distanceToPlayer)
    {
        if (isHurt || isDead) return;


        float yDifference = Mathf.Abs(player.position.y - transform.position.y);
        float xDistance = Mathf.Abs(player.position.x - transform.position.x);


        if (yDifference <= allowedYDifference)
        {
            if (xDistance <= attackRange)
            {
                currentState = EnemyState.Attack;
            }
            else if (xDistance <= detectionRangeX)
            {
                currentState = EnemyState.Chase;
            }
            else
            {

                if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
                {
                    currentState = EnemyState.Patrol;
                    SetPatrolTarget(transform.position.x < patrolCenter.x);
                }
                else if (currentState != EnemyState.Patrol && currentState != EnemyState.Idle)
                {
                    currentState = EnemyState.Patrol;
                    SetPatrolTarget(transform.position.x < patrolCenter.x);
                }
            }
        }
        else
        {

            if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
            {
                currentState = EnemyState.Patrol;
                SetPatrolTarget(transform.position.x < patrolCenter.x);
            }
            else if (currentState != EnemyState.Patrol && currentState != EnemyState.Idle)
            {
                currentState = EnemyState.Patrol;
                SetPatrolTarget(transform.position.x < patrolCenter.x);
            }
        }
    }

    void Idle()
    {
        if (isHurt || isDead) return;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }


    void Patrol()
    {
        if (isHurt || isDead)
        {

            if (rb.linearVelocity.x != 0)
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }


        if (groundCheck != null && !Physics2D.OverlapCircle(groundCheck.position, groundCheckDistance, 1 << LayerMask.NameToLayer("Ground")))
        {

            SetPatrolTarget(!patrollingRight);



            return;
        }



        float directionX = Mathf.Sign(currentPatrolTarget.x - transform.position.x);


        rb.linearVelocity = new Vector2(directionX * patrolSpeed, rb.linearVelocity.y);



        if (Mathf.Abs(transform.position.x - currentPatrolTarget.x) < 0.1f)
        {

            SetPatrolTarget(!patrollingRight);

            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }



    }


    void SetPatrolTarget(bool goRight)
    {
        patrollingRight = goRight;
        float targetX = patrolCenter.x + (goRight ? patrolRange : -patrolRange);
        currentPatrolTarget = new Vector3(targetX, transform.position.y);


        UpdateFacingDirectionTowards(currentPatrolTarget);

    }





    void Chase()
    {
        if (isHurt || isDead) return;
        Vector2 direction = (player.position - transform.position).normalized;

        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
        UpdateFacingDirectionTowards(player.position);
    }

    void Attack()
    {
        if (isHurt || isDead) return;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        UpdateFacingDirectionTowards(player.position);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger(hashAttack);



        }
    }

    public void AnimationEvent_DealDamage()
    {
        if (isDead) return;

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, 1 << LayerMask.NameToLayer("Player"));
        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {

                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void AnimationEvent_AttackEnd() { }

    void UpdateAnimator()
    {
        bool isActuallyWalking = (currentState == EnemyState.Patrol || currentState == EnemyState.Chase) && Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        animator.SetBool(hashIsWalking, isActuallyWalking && !isHurt && !isDead);
    }

    void UpdateFacingDirectionTowards(Vector3 targetPosition)
    {
        if (isHurt || isDead) return;

        if (targetPosition.x > transform.position.x && !facingRight)
        {
            Flip();
        }
        else if (targetPosition.x < transform.position.x && facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }


    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.yellow;

        Vector3 detectionSize = new Vector3(detectionRangeX * 2, allowedYDifference * 2, 0.1f);
        Gizmos.DrawWireCube(transform.position, detectionSize);


        Gizmos.color = Color.red;
        Vector3 attackSize = new Vector3(attackRange * 2, allowedYDifference * 2, 0.1f);
        Gizmos.DrawWireCube(transform.position, attackSize);


        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }


        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Vector3 rightEdge = patrolCenter + Vector3.right * patrolRange;
            Vector3 leftEdge = patrolCenter + Vector3.left * patrolRange;
            Gizmos.DrawLine(leftEdge + Vector3.up * 0.2f, rightEdge + Vector3.up * 0.2f);
            Gizmos.DrawSphere(leftEdge, 0.15f);
            Gizmos.DrawSphere(rightEdge, 0.15f);


            if (currentState == EnemyState.Patrol && !isHurt && !isDead)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, currentPatrolTarget);
            }
        }
        else
        {
            Gizmos.color = Color.blue;
            Vector3 currentPos = transform.position;
            Vector3 rightEdge = currentPos + Vector3.right * patrolRange;
            Vector3 leftEdge = currentPos + Vector3.left * patrolRange;
            Gizmos.DrawLine(leftEdge + Vector3.up * 0.2f, rightEdge + Vector3.up * 0.2f);
            Gizmos.DrawSphere(leftEdge, 0.15f);
            Gizmos.DrawSphere(rightEdge, 0.15f);
        }



        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckDistance);
        }
    }
}