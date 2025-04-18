using UnityEngine;
using System.Collections;

public class EnemyQuaiBay : MonoBehaviour
{
    public enum QuaiBayState { Idle, Patrol, Chase, Attack, Hurt, Die }

    [Header("State Machine")]
    [SerializeField] private QuaiBayState currentState = QuaiBayState.Patrol;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D enemyCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Health")]
    private int currentHealth;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float diveSpeed = 8f;
    [SerializeField] private float flyHeight = 4f;
    [SerializeField] private float patrolHeight = 3f;
    [SerializeField] private float patrolHorizontalDistance = 5f;
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackLateralDistance = 1.5f;
    [SerializeField] private float returnToPatrolDelay = 3f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip swooshSound;

    [Header("Feedback & Death")]
    [SerializeField] private float hurtDuration = 0.3f;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private float destroyDelay = 2f;

    private Transform player;
    private Vector3 startPosition;
    private Vector3 patrolLeftPoint;
    private Vector3 patrolRightPoint;
    private bool movingRight = true;
    private float lastAttackTime = -Mathf.Infinity;
    private bool isHurt = false;
    private bool isDead = false;
    private bool isGrounded = false;
    private bool isStayingOnGround = false;
    private float timeOnGround = 0f;
    private float groundStayDuration = 1.5f;
    private AudioSource audioSource;
    private Vector3 diveTargetPosition;


    private readonly int hashIsFlying = Animator.StringToHash("IsFlying");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashDeath = Animator.StringToHash("Death");

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
            Debug.LogError($"QuaiBay '{gameObject.name}': Cannot find GameObject with Tag 'Player'. Disabling script.", this);
            enabled = false;
            return;
        }
    }

    void Start()
    {

        startPosition = transform.position;


        patrolLeftPoint = startPosition + Vector3.left * patrolHorizontalDistance;
        patrolRightPoint = startPosition + Vector3.right * patrolHorizontalDistance;


        MoveToHeight(patrolHeight);

        currentHealth = 2;
        rb.gravityScale = 0;
    }

    void Update()
    {
        if (isDead) return;

        if (isHurt) return;


        CheckGrounded();


        if (isGrounded && isStayingOnGround)
        {
            timeOnGround += Time.deltaTime;
            if (timeOnGround >= groundStayDuration)
            {
                TakeOff();
            }
        }

        if (currentState != QuaiBayState.Die)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            UpdateCurrentState(distanceToPlayer);

            switch (currentState)
            {
                case QuaiBayState.Idle:
                    Idle();
                    break;
                case QuaiBayState.Patrol:
                    Patrol();
                    break;
                case QuaiBayState.Chase:
                    Chase();
                    break;
                case QuaiBayState.Attack:
                    Attack();
                    break;
            }
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
        if (isGrounded && isStayingOnGround) return;


        if (distanceToPlayer <= detectionRange)
        {

            if (distanceToPlayer <= attackRange)
            {

                if (currentState != QuaiBayState.Attack)
                {
                    currentState = QuaiBayState.Attack;
                }
            }

            else
            {

                currentState = QuaiBayState.Chase;
            }
        }

        else
        {

            if (currentState != QuaiBayState.Patrol && currentState != QuaiBayState.Idle)
            {
                currentState = QuaiBayState.Patrol;
            }
        }
    }

    void Idle()
    {
        rb.linearVelocity = new Vector2(0, 0);
    }

    void Patrol()
    {

        float currentHeight = transform.position.y;
        float targetHeight = startPosition.y + patrolHeight;
        float verticalSpeed = patrolSpeed * 0.5f;

        float verticalVelocity = 0f;
        if (Mathf.Abs(currentHeight - targetHeight) > 0.1f)
        {

            verticalVelocity = (targetHeight > currentHeight) ? verticalSpeed : -verticalSpeed;
        }


        Vector3 horizontalTarget;


        if (movingRight)
        {
            horizontalTarget = patrolRightPoint;

            if (Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(patrolRightPoint.x, 0)) < 0.5f)
            {
                movingRight = false;
                horizontalTarget = patrolLeftPoint;
            }
        }
        else
        {
            horizontalTarget = patrolLeftPoint;

            if (Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(patrolLeftPoint.x, 0)) < 0.5f)
            {
                movingRight = true;
                horizontalTarget = patrolRightPoint;
            }
        }


        float horizontalDistance = horizontalTarget.x - transform.position.x;
        float horizontalDirection = Mathf.Sign(horizontalDistance);
        float horizontalVelocity = horizontalDirection * patrolSpeed;


        rb.linearVelocity = new Vector2(horizontalVelocity, verticalVelocity);


        UpdateSpriteDirection(horizontalDirection);
    }

    void Chase()
    {

        if (transform.position.y < patrolHeight)
        {
            MoveToHeight(patrolHeight);
        }


        Vector2 direction = new Vector2(player.position.x - transform.position.x, 0).normalized;
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);


        UpdateSpriteDirection(direction.x);
    }

    void Attack()
    {

        if (Time.time >= lastAttackTime + attackCooldown && !isGrounded)
        {

            StartCoroutine(DiveAttack());
        }
    }

    IEnumerator DiveAttack()
    {
        lastAttackTime = Time.time;


        float targetSide = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;


        diveTargetPosition = new Vector3(
            player.position.x + (attackLateralDistance * targetSide),
            player.position.y,
            player.position.z);


        RaycastHit2D groundCheck = Physics2D.Raycast(
            new Vector2(diveTargetPosition.x, diveTargetPosition.y + 5f),
            Vector2.down,
            10f,
            groundLayer);


        if (groundCheck.collider == null)
        {
            targetSide *= -1;
            diveTargetPosition = new Vector3(
                player.position.x + (attackLateralDistance * targetSide),
                player.position.y,
                player.position.z);
        }


        if (swooshSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(swooshSound);
        }


        rb.gravityScale = 0.5f;


        if (rb.collisionDetectionMode != CollisionDetectionMode2D.Continuous)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }


        float flyTime = 0.5f;
        float elapsed = 0;
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(diveTargetPosition.x, transform.position.y, transform.position.z);

        while (elapsed < flyTime)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / flyTime);


            UpdateSpriteDirection(player.position.x - transform.position.x);
            yield return null;
        }


        rb.linearVelocity = new Vector2(0, -diveSpeed);


        animator.SetTrigger(hashAttack);


        float timeout = 2.0f;
        float timer = 0;

        while (!isGrounded && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }


        if (isGrounded)
        {

            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
            }




            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 1;

            isStayingOnGround = true;
            timeOnGround = 0;
        }
        else
        {

            TakeOff();
        }
    }


    public void AnimationEvent_DealDamage()
    {
        if (isDead) return;


        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }


        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(
            attackPoint != null ? attackPoint.position : transform.position,
            attackRadius,
            1 << LayerMask.NameToLayer("Player"));

        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    void TakeOff()
    {
        isStayingOnGround = false;
        rb.gravityScale = 0;


        Vector2 riseVelocity = new Vector2(0, patrolSpeed);
        rb.linearVelocity = riseVelocity;


        StartCoroutine(ReturnToPatrolAfterDelay());
    }

    IEnumerator ReturnToPatrolAfterDelay()
    {
        yield return new WaitForSeconds(returnToPatrolDelay);
        currentState = QuaiBayState.Patrol;
    }

    void CheckGrounded()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    void MoveToHeight(float height)
    {

        Vector3 targetPos = new Vector3(transform.position.x, startPosition.y + height, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * patrolSpeed);
    }

    void UpdateSpriteDirection(float directionX)
    {

        if (directionX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (directionX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
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
            StartCoroutine(HurtSequence());
        }
    }

    IEnumerator HurtSequence()
    {
        isHurt = true;
        rb.linearVelocity = Vector2.zero;


        animator.SetTrigger(hashHurt);


        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        yield return new WaitForSeconds(hurtDuration);
        isHurt = false;
    }

    void Die()
    {

        isDead = true;
        currentState = QuaiBayState.Die;


        rb.linearVelocity = Vector2.zero;


        rb.gravityScale = 1;


        animator.SetTrigger(hashDeath);


        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }


        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }


        enemyCollider.enabled = false;


        Destroy(gameObject, destroyDelay);
    }

    void UpdateAnimator()
    {

        animator.SetBool(hashIsFlying, !isGrounded);
    }

    public void AnimationEvent_AttackEnd()
    {


        if (!isDead)
        {
            AnimationEvent_DealDamage();
        }
    }

    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);


        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(patrolLeftPoint, patrolRightPoint);
            Gizmos.DrawSphere(patrolLeftPoint, 0.2f);
            Gizmos.DrawSphere(patrolRightPoint, 0.2f);


            if (currentState == QuaiBayState.Attack)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(diveTargetPosition, 0.3f);
            }
        }
        else
        {

            Vector3 leftPoint = transform.position + Vector3.left * patrolHorizontalDistance;
            Vector3 rightPoint = transform.position + Vector3.right * patrolHorizontalDistance;
            leftPoint.y = transform.position.y + patrolHeight;
            rightPoint.y = transform.position.y + patrolHeight;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftPoint, rightPoint);
            Gizmos.DrawSphere(leftPoint, 0.2f);
            Gizmos.DrawSphere(rightPoint, 0.2f);


            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x + attackLateralDistance, transform.position.y - patrolHeight, transform.position.z), 0.3f);
            Gizmos.DrawWireSphere(new Vector3(transform.position.x - attackLateralDistance, transform.position.y - patrolHeight, transform.position.z), 0.3f);
        }


        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }


        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}