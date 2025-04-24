using UnityEngine;
using System.Collections;

public class Lizard: MonoBehaviour
{
    public enum EnemyState { Idle, Walk, Hurt, Death, Attack, JumpTowardsPlayer, RetreatAfterAttack, JumpBackFurther }
    [Header("State Machine")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D enemyCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Movement & Detection")]
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float patrolRange = 3f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private AudioClip attackSound;

    [Header("Special Behavior")]
    [SerializeField] private float closeDistance = 1f;
    [SerializeField] private float retreatDistance = 2f;
    [SerializeField] private float jumpBackDistance = 5f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float attackDelay = 1f;
    [SerializeField] private float attackDuration = 1f;

    private Transform player;
    private float lastAttackTime = -Mathf.Infinity;
    private float lastActionTime = -Mathf.Infinity;
    private bool isHurt = false;
    private bool isDead = false;
    private bool facingRight = true;
    private AudioSource audioSource;
    private Vector3 patrolCenter;
    private Vector3 currentPatrolTarget;
    private bool patrollingRight = true;
    private bool isGrounded = true;
    private Coroutine currentActionCoroutine;

    [Header("Cooldown")]
    [SerializeField] private float actionCooldown = 2.5f;


    private readonly int hashIsWalking = Animator.StringToHash("IsWalking");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashDie = Animator.StringToHash("Die");

    void Awake()
    {

        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (enemyCollider == null) enemyCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();


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

        SetState(EnemyState.Idle);
    }

    void Update()
    {
        if (isDead || player == null)
        {
            return;
        }


        CheckGrounded();


        if (isHurt)
        {
            UpdateAnimator();
            return;
        }


        if (isGrounded ||
            currentState == EnemyState.JumpTowardsPlayer ||
            currentState == EnemyState.RetreatAfterAttack ||
            currentState == EnemyState.JumpBackFurther)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (currentState == EnemyState.Idle || currentState == EnemyState.Walk)
            {
                UpdateCurrentState(distanceToPlayer);
            }
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Walk:
                Walk();
                break;
            case EnemyState.Attack:

                break;
            case EnemyState.JumpTowardsPlayer:
            case EnemyState.RetreatAfterAttack:
            case EnemyState.JumpBackFurther:

                break;
        }

        UpdateAnimator();
    }

    void CheckGrounded()
    {

        bool wasGrounded = isGrounded;


        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckDistance, groundLayer);
        }


        if (!isGrounded && GetComponent<Collider2D>() != null)
        {
            Vector2 center = transform.position - new Vector3(0, GetComponent<Collider2D>().bounds.extents.y - 0.05f, 0);
            Vector2 left = center - new Vector2(GetComponent<Collider2D>().bounds.extents.x * 0.8f, 0);
            Vector2 right = center + new Vector2(GetComponent<Collider2D>().bounds.extents.x * 0.8f, 0);

            float rayLength = 0.15f;

            RaycastHit2D hitCenter = Physics2D.Raycast(center, Vector2.down, rayLength, groundLayer);
            RaycastHit2D hitLeft = Physics2D.Raycast(left, Vector2.down, rayLength, groundLayer);
            RaycastHit2D hitRight = Physics2D.Raycast(right, Vector2.down, rayLength, groundLayer);




            if (hitCenter || hitLeft || hitRight)
            {
                isGrounded = true;
            }
        }


        if (!isGrounded && GetComponent<Collider2D>() != null)
        {
            Bounds bounds = GetComponent<Collider2D>().bounds;
            Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y + 0.1f);
            Vector2 boxSize = new Vector2(bounds.size.x * 0.9f, 0.1f);

            Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, groundLayer);
            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject != gameObject)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }

    void UpdateCurrentState(float distanceToPlayer)
    {
        if (isHurt || isDead) return;


        if (Time.time - lastActionTime < actionCooldown)
        {
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            if (currentActionCoroutine != null)
            {
                StopCoroutine(currentActionCoroutine);
            }
            currentActionCoroutine = StartCoroutine(JumpTowardsPlayerCoroutine());
        }
        else
        {
            SetState(EnemyState.Walk);
        }
    }

    void SetState(EnemyState newState)
    {
        currentState = newState;
    }

    void Idle()
    {
        if (isHurt || isDead) return;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void Walk()
    {
        if (isHurt || isDead) return;


        if (!isGrounded)
        {
            return;
        }


        if (currentPatrolTarget == Vector3.zero)
        {
            SetPatrolTarget(true);
        }

        float directionX = Mathf.Sign(currentPatrolTarget.x - transform.position.x);
        rb.linearVelocity = new Vector2(directionX * walkSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(transform.position.x - currentPatrolTarget.x) < 0.1f)
        {
            SetPatrolTarget(!patrollingRight);
        }

        UpdateFacingDirection(currentPatrolTarget);
    }

    void SetPatrolTarget(bool goRight)
    {
        patrollingRight = goRight;
        float targetX = patrolCenter.x + (goRight ? patrolRange : -patrolRange);
        currentPatrolTarget = new Vector3(targetX, transform.position.y);
    }

    IEnumerator SwitchToWalkAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isHurt && !isDead && currentState == EnemyState.Idle)
        {
            SetState(EnemyState.Walk);
        }
    }

    IEnumerator JumpTowardsPlayerCoroutine()
    {
        SetState(EnemyState.JumpTowardsPlayer);

        UpdateFacingDirection(player.position);

        Vector2 playerPosition = player.position;
        float directionToPlayer = Mathf.Sign(playerPosition.x - transform.position.x);

        Vector2 targetPosition = new Vector2(playerPosition.x - directionToPlayer * 4f, transform.position.y);


        if (isHurt || isDead)
        {
            yield break;
        }

        yield return StartCoroutine(JumpToPosition(targetPosition));


        bool reachedAttackRange = false;
        float approachTimeout = 3f;
        float approachStartTime = Time.time;

        while (!reachedAttackRange && Time.time - approachStartTime < approachTimeout)
        {

            playerPosition = player.position;
            directionToPlayer = Mathf.Sign(playerPosition.x - transform.position.x);


            rb.linearVelocity = new Vector2(directionToPlayer * walkSpeed, rb.linearVelocity.y);


            UpdateFacingDirection(playerPosition);


            float distanceToPlayer = Vector2.Distance(transform.position, playerPosition);
            if (distanceToPlayer <= attackRange)
            {
                reachedAttackRange = true;
            }

            yield return null;
        }


        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);


        if (reachedAttackRange)
        {

            yield return new WaitForSeconds(attackDelay);


            yield return StartCoroutine(PerformAttack());


            yield return StartCoroutine(RetreatAfterAttack());


            yield return StartCoroutine(JumpBackFurther());
        }



        lastActionTime = Time.time;


        SetState(EnemyState.Idle);


        StartCoroutine(SwitchToWalkAfterDelay(1.0f));

        currentActionCoroutine = null;
    }

    IEnumerator JumpToPosition(Vector2 targetPosition)
    {

        if (isHurt || isDead)
        {
            yield break;
        }

        float waitStartTime = Time.time;
        float maxWaitTime = 1.5f;


        while (!isGrounded && Time.time - waitStartTime < maxWaitTime)
        {

            if (isHurt || isDead)
            {
                yield break;
            }

            CheckGrounded();
            yield return null;
        }


        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);


        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetPosition);


        float maxJumpDistance = 5.0f;
        if (distance > maxJumpDistance)
        {
            targetPosition = (Vector2)transform.position + direction * maxJumpDistance;
            distance = maxJumpDistance;
        }


        float horizontalForce = direction.x * jumpSpeed;


        float verticalForce = Mathf.Min(jumpForce, 4f);


        if (distance < 3.0f)
        {
            horizontalForce *= 1f;
            verticalForce *= 0.5f;
        }


        rb.linearVelocity = new Vector2(horizontalForce, verticalForce);


        float startTime = Time.time;
        float maxJumpTime = 0.8f;
        bool hasJumped = false;

        while (Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(targetPosition.x, 0)) > 0.5f)
        {

            if (Time.time - startTime > maxJumpTime)
            {
                break;
            }


            if (!hasJumped && rb.linearVelocity.y > 0)
            {
                hasJumped = true;
            }


            if (hasJumped && isGrounded && rb.linearVelocity.y <= 0)
            {
                break;
            }


            if (rb.linearVelocity.y < -8f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -8f);
            }


            CheckGrounded();


            UpdateFacingDirection(targetPosition);

            yield return null;
        }


        float groundWaitStart = Time.time;
        while (!isGrounded && Time.time - groundWaitStart < 0.5f)
        {
            CheckGrounded();
            yield return null;
        }


        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    IEnumerator PerformAttack()
    {

        SetState(EnemyState.Attack);


        animator.SetTrigger(hashAttack);


        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }


        yield return new WaitForSeconds(attackDuration / 2);


        DealDamage();


        yield return new WaitForSeconds(attackDuration / 2);
    }

    void DealDamage()
    {

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

    IEnumerator RetreatAfterAttack()
    {
        SetState(EnemyState.RetreatAfterAttack);

        yield return new WaitForSeconds(0.2f);


        if (isHurt || isDead)
        {
            yield break;
        }

        CheckGrounded();
        if (!isGrounded)
        {
            float waitStartTime = Time.time;
            while (!isGrounded && Time.time - waitStartTime < 1.0f)
            {

                if (isHurt || isDead)
                {
                    yield break;
                }

                CheckGrounded();
                yield return null;
            }
        }

        Vector2 playerPosition = player.position;
        float directionToPlayer = Mathf.Sign(playerPosition.x - transform.position.x);
        Vector2 retreatPosition = new Vector2(playerPosition.x - directionToPlayer * retreatDistance, transform.position.y);

        yield return StartCoroutine(JumpToPosition(retreatPosition));
    }

    IEnumerator JumpBackFurther()
    {
        SetState(EnemyState.JumpBackFurther);

        yield return new WaitForSeconds(0.3f);


        if (isHurt || isDead)
        {
            yield break;
        }

        CheckGrounded();
        float waitStartTime = Time.time;
        while (!isGrounded && Time.time - waitStartTime < 1.5f)
        {

            if (isHurt || isDead)
            {
                yield break;
            }

            CheckGrounded();
            yield return null;
        }


        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);

        Vector2 playerPosition = player.position;
        float directionToPlayer = Mathf.Sign(playerPosition.x - transform.position.x);
        Vector2 jumpBackPosition = new Vector2(
            transform.position.x - directionToPlayer * jumpBackDistance,
            transform.position.y
        );

        yield return StartCoroutine(JumpToPosition(jumpBackPosition));
    }

    void UpdateAnimator()
    {

        bool isMoving = (currentState == EnemyState.Walk ||
                         currentState == EnemyState.JumpTowardsPlayer ||
                         currentState == EnemyState.RetreatAfterAttack ||
                         currentState == EnemyState.JumpBackFurther) &&
                         Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        animator.SetBool(hashIsWalking, isMoving && !isHurt && !isDead);
    }

    void UpdateFacingDirection(Vector3 targetPosition)
    {
        if (isHurt || isDead) return;

        bool shouldFaceRight = targetPosition.x > transform.position.x;

        if (shouldFaceRight != facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HurtCoroutine());
        }
    }

    IEnumerator HurtCoroutine()
    {

        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }

        isHurt = true;
        SetState(EnemyState.Hurt);

        animator.SetTrigger(hashHurt);


        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.5f);

        isHurt = false;
        SetState(EnemyState.Idle);
    }

    void Die()
    {

        isDead = true;
        SetState(EnemyState.Death);


        animator.SetTrigger(hashDie);


        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }


        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;


        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);


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


            if (currentState == EnemyState.Walk && !isHurt && !isDead)
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


        if (player != null && Application.isPlaying)
        {

            Vector3 attackPos = player.position - (player.position - transform.position).normalized * closeDistance;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawSphere(new Vector3(attackPos.x, transform.position.y, attackPos.z), 0.3f);


            Vector3 retreatPos = player.position - (player.position - transform.position).normalized * retreatDistance;
            Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.5f);
            Gizmos.DrawSphere(new Vector3(retreatPos.x, transform.position.y, retreatPos.z), 0.3f);


            Vector3 jumpBackPos = retreatPos - (player.position - transform.position).normalized * jumpBackDistance;
            Gizmos.color = new Color(0f, 0.5f, 0.5f, 0.5f);
            Gizmos.DrawSphere(new Vector3(jumpBackPos.x, transform.position.y, jumpBackPos.z), 0.3f);
        }
    }
}