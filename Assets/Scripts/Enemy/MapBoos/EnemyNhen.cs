﻿using UnityEngine;
using System.Collections;

public class EnemyNhen : MonoBehaviour
{
    public enum NhenState { Idle, Walk, Attack, Hurt, Die }

    [Header("State Machine")]
    [SerializeField] private NhenState currentState = NhenState.Idle;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D enemyCollider;
    [SerializeField] private Transform groundCheck;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float patrolRange = 5f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float edgeCheckDistance = 1f;

    [Header("Detection & Attack")]
    [SerializeField] private float detectionRangeX = 8f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float allowedYDifference = 1.5f;
    [SerializeField] private Transform webShootPoint;
    [SerializeField] private GameObject webPrefab;
    [SerializeField] private float webSpeed = 7f;
    [SerializeField] private int webDamage = 5;
    [SerializeField] private float webArcHeight = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip webShootSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;

    private Transform player;
    private Vector3 startPosition;
    private bool facingRight = true;
    private float lastAttackTime = -999f;
    private bool isHurt = false;
    private bool isDead = false;
    private AudioSource audioSource;
    private bool isAttacking = false;

    private Vector3 leftPatrolPoint;
    private Vector3 rightPatrolPoint;
    private bool movingRight = true;

    private readonly int hashIsWalking = Animator.StringToHash("IsWalking");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashDie = Animator.StringToHash("Death");

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (enemyCollider == null) enemyCollider = GetComponent<Collider2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            enabled = false;
            return;
        }
    }

    void Start()
    {
        startPosition = transform.position;
        leftPatrolPoint = startPosition - Vector3.right * patrolRange;
        rightPatrolPoint = startPosition + Vector3.right * patrolRange;
    }

    void Update()
    {
        if (isDead || isHurt) return;

        float xDistance = Mathf.Abs(player.position.x - transform.position.x);
        float yDifference = Mathf.Abs(player.position.y - transform.position.y);

        UpdateFacingDirection();

        UpdateState(xDistance, yDifference);

        switch (currentState)
        {
            case NhenState.Idle:
                Idle();
                break;
            case NhenState.Walk:
                Walk();
                break;
            case NhenState.Attack:
                Attack();
                break;
        }

        UpdateAnimator();
    }

    void UpdateState(float xDistance, float yDifference)
    {
        if (isDead || isHurt) return;

        if (isAttacking)
        {
            return;
        }

        NhenState previousState = currentState;

        // Chỉ phát hiện và tấn công khi người chơi ở gần cùng độ cao
        if (yDifference <= allowedYDifference)
        {
            if (xDistance <= attackRange)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    currentState = NhenState.Attack;
                }
            }
            else if (xDistance <= detectionRangeX)
            {
                currentState = NhenState.Walk;
            }
            else
            {
                if (currentState != NhenState.Walk && currentState != NhenState.Idle)
                {
                    currentState = NhenState.Walk;
                }
            }
        }
        else
        {
            // Người chơi không ở cùng độ cao, quay lại tuần tra
            if (currentState != NhenState.Walk && currentState != NhenState.Idle)
            {
                currentState = NhenState.Walk;
            }
        }
    }

    void Idle()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void Walk()
    {
        float xDistance = Mathf.Abs(player.position.x - transform.position.x);
        float yDifference = Mathf.Abs(player.position.y - transform.position.y);

        if (xDistance <= detectionRangeX && yDifference <= allowedYDifference)
        {
            MoveTowardsPlayer();
        }
        else
        {
            Patrol();
        }
    }

    void MoveTowardsPlayer()
    {
        float distanceToPlayer = player.position.x - transform.position.x;
        float direction = Mathf.Sign(distanceToPlayer);

        if (CanMoveInDirection(direction))
        {
            rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
            UpdateFacingDirection(direction);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void Patrol()
    {
        Vector3 targetPoint = movingRight ? rightPatrolPoint : leftPatrolPoint;
        float direction = movingRight ? 1 : -1;

        if (!CanMoveInDirection(direction))
        {
            movingRight = !movingRight;
            direction = -direction;
        }

        float distanceToTarget = Mathf.Abs(transform.position.x - targetPoint.x);
        if (distanceToTarget < 0.1f)
        {
            movingRight = !movingRight;
        }

        rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);

        UpdateFacingDirection(direction);
    }

    bool CanMoveInDirection(float direction)
    {
        float groundCheckDepth = 2f;

        Vector2 checkPosition = (Vector2)transform.position + new Vector2(direction * edgeCheckDistance, 0);

        RaycastHit2D groundInfo = Physics2D.BoxCast(
            checkPosition,
            new Vector2(0.3f, 0.1f),
            0f,
            Vector2.down,
            groundCheckDepth
        );

        bool hasGroundAhead = false;

        if (groundInfo.collider != null && groundInfo.collider.CompareTag("Ground"))
        {
            hasGroundAhead = true;
        }
        else if (groundInfo.collider != null)
        {
            hasGroundAhead = true;
        }
        else
        {
            RaycastHit2D backupGroundInfo = Physics2D.Raycast(checkPosition, Vector2.down, groundCheckDepth);
            if (backupGroundInfo.collider != null)
            {
                hasGroundAhead = true;
            }
        }

        Vector2 wallCheckPosition = (Vector2)transform.position + new Vector2(0, 0.5f);

        RaycastHit2D wallInfo = Physics2D.BoxCast(
            wallCheckPosition,
            new Vector2(0.1f, 0.5f),
            0f,
            new Vector2(direction, 0),
            0.7f
        );

        bool hasWallAhead = false;
        if (wallInfo.collider != null)
        {
            if (!wallInfo.collider.CompareTag("Player") && !wallInfo.collider.isTrigger)
            {
                hasWallAhead = true;
            }
        }

        bool isOnGround = Physics2D.Raycast(transform.position, Vector2.down, 0.3f).collider != null;

        return (hasGroundAhead && !hasWallAhead) || isOnGround;
    }

    void Attack()
    {
        if (isDead || isHurt) return;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        UpdateFacingDirection(directionToPlayer);

        if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            StartCoroutine(AttackSequence());
        }
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;

        animator.SetTrigger(hashAttack);

        float attackAnimDuration = 1.0f;

        yield return new WaitForSeconds(attackAnimDuration);

        AnimationEvent_ShootWeb();

        isAttacking = false;
    }

    public void AnimationEvent_ShootWeb()
    {
        if (isDead) return;

        if (webShootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(webShootSound);
        }

        SpawnWeb();
    }

    void SpawnWeb()
    {
        if (webPrefab != null && webShootPoint != null)
        {

            GameObject web = Instantiate(webPrefab, webShootPoint.position, Quaternion.identity);

            FireballProjectile fireballScript = web.GetComponent<FireballProjectile>();
            if (fireballScript != null)
            {
                Destroy(fireballScript); // Xóa FireballProjectile để tránh trùng lặp sát thương
            }

            WebProjectile webScript = web.GetComponent<WebProjectile>();
            if (webScript == null)
            {
                webScript = web.AddComponent<WebProjectile>();
            }

            float direction = facingRight ? 1 : -1;
            webScript.Initialize(direction, webSpeed, webDamage, gameObject);

            Collider2D webCollider = web.GetComponent<Collider2D>();
            if (webCollider != null && enemyCollider != null)
            {
                Physics2D.IgnoreCollision(webCollider, enemyCollider, true);
            }

            if (!facingRight)
            {
                web.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else
        {
        }
    }

    void UpdateFacingDirection()
    {
        if (player != null)
        {
            float direction = player.position.x - transform.position.x;
            if (direction != 0)
            {
                UpdateFacingDirection(Mathf.Sign(direction));
            }
        }
    }

    void UpdateFacingDirection(float direction)
    {
        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void UpdateAnimator()
    {
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        animator.SetBool(hashIsWalking, isMoving);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        StartCoroutine(HurtSequence());
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

        yield return new WaitForSeconds(0.5f);
        isHurt = false;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger(hashDie);

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        enemyCollider.enabled = false;

        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        // Hiển thị vùng phát hiện dướidạng hình chữ nhật
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(detectionRangeX * 2, allowedYDifference * 2, 0.1f)
        );

        // Hiển thị vùng tấn công dướidạng hình chữ nhật
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(attackRange * 2, allowedYDifference * 2, 0.1f)
        );

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(leftPatrolPoint, rightPatrolPoint);
            Gizmos.DrawSphere(leftPatrolPoint, 0.2f);
            Gizmos.DrawSphere(rightPatrolPoint, 0.2f);
        }
        else
        {
            Vector3 leftPoint = transform.position - Vector3.right * patrolRange;
            Vector3 rightPoint = transform.position + Vector3.right * patrolRange;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(leftPoint, rightPoint);
            Gizmos.DrawSphere(leftPoint, 0.2f);
            Gizmos.DrawSphere(rightPoint, 0.2f);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }

        if (webShootPoint != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(webShootPoint.position, 0.1f);

            Vector3 direction = (facingRight ? Vector3.right : Vector3.left);
            Gizmos.DrawRay(webShootPoint.position, direction * 2f);

            Vector3 arcDirection = direction + Vector3.up;
            Gizmos.DrawRay(webShootPoint.position, arcDirection.normalized * 2f);
        }
    }

    private void OnValidate()
    {

        if (webPrefab == null)
        {
        }

        if (webShootPoint == null)
        {
        }
    }
}