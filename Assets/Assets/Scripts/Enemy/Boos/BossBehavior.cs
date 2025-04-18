using UnityEngine;
using System.Collections;

public class BossBehavior : MonoBehaviour
{
    // Các trạng thái của boss
    public enum BossState { Idle, Patrol, Chase, Attack, MagicBlade, MagicFire, Death }

    public BossState currentState = BossState.Patrol;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3f;
    public float detectionRange = 7f;
    public float attackRange = 2f;

    [Header("Attack Settings")]
    public int attackDamage = 2;
    public float attackCooldown = 1.5f;
    public float attackDelayBeforeFirstHit = 0.5f;

    [Header("Magic Settings")]
    public int magicBladeDamage = 3;
    public float magicBladeCooldown = 4f;
    public float magicBladeRange = 3f;
    public int magicFireDamage = 5;
    public float magicFireCooldown = 6f;
    public float magicFireRange = 5f;

    [Header("Special States")]
    public float angerHealthThreshold = 0.3f; // Trigger anger when health below 30%
    public float angerDamageMultiplier = 1.5f;
    public float angerSpeedMultiplier = 1.3f;

    [Header("Patrol Settings")]
    public Transform patrolCenter;
    public int patrolRange = 5;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;

    private float lastAttackTime;
    private float lastMagicBladeTime;
    private float lastMagicFireTime;
    private float enteredAttackRangeTime = -Mathf.Infinity;
    private Vector3 patrolTarget;
    private bool movingRight = true;
    private bool isAngry = false;
    private bool isDead = false;
    private bool isPlayingSpecialAnimation = false;
    private float angerAnimationEndTime;

    // Biến mới cho hành vi thông minh
    private bool playerDetected = false; // Đã phát hiện player chưa
    private bool isRepositioning = false; // Đang di chuyển đến vị trí mới
    private Vector3 repositionTarget; // Vị trí mục tiêu khi di chuyển
    private float preferredMagicDistance = 4f; // Khoảng cách ưa thích để sử dụng phép thuật

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();


        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (patrolCenter == null)
        {
            GameObject center = new GameObject("BossPatrolCenter");
            center.transform.position = transform.position;
            patrolCenter = center.transform;
        }

        patrolTarget = patrolCenter.position + Vector3.right * patrolRange;
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceToPatrolCenter = Vector2.Distance(player.position, patrolCenter.position);

        // Phát hiện player khi vào vùng tuần tra và kích hoạt trạng thái tức giận
        if (!playerDetected && distanceToPatrolCenter <= patrolRange)
        {
            playerDetected = true; // Đánh dấu đã phát hiện player
            EnterAngerState(); // Kích hoạt trạng thái tức giận
        }

        // Phát hiện player khi ở trong tầm phát hiện
        if (!playerDetected && distanceToPlayer <= detectionRange)
        {
            playerDetected = true;
        }

        // Kiểm tra nếu đang chạy animation đặc biệt
        if (isPlayingSpecialAnimation)
        {
            // Kiểm tra xem animation đặc biệt đã kết thúc chưa
            if (Time.time >= angerAnimationEndTime)
            {
                isPlayingSpecialAnimation = false;
            }
            else
            {
                // Vẫn cho phép di chuyển trong khi chạy animation đặc biệt
                if (playerDetected)
                {
                    Chase();
                }
                return;
            }
        }

        // Nếu đang di chuyển đến vị trí mới
        if (isRepositioning)
        {
            MoveToPosition();

            // Kiểm tra xem đã đến vị trí mục tiêu chưa
            if (Vector2.Distance(transform.position, repositionTarget) < 0.5f)
            {
                isRepositioning = false;

                // Đã đến vị trí tốt, sử dụng kỹ năng phù hợp
                if (Time.time - lastMagicFireTime >= magicFireCooldown)
                {
                    currentState = BossState.MagicFire;
                }
                else if (Time.time - lastMagicBladeTime >= magicBladeCooldown)
                {
                    currentState = BossState.MagicBlade;
                }
            }
            return;
        }

        UpdateState(distanceToPlayer);

        switch (currentState)
        {
            case BossState.Idle:
                Idle();
                break;
            case BossState.Patrol:
                Patrol();
                break;
            case BossState.Chase:
                // Kiểm tra xem có nên di chuyển ra xa để sử dụng phép thuật không
                if (playerDetected && ShouldRepositionForMagic(distanceToPlayer))
                {
                    RepositionForMagicAttack();
                }
                else
                {
                    Chase();
                }
                break;
            case BossState.Attack:
                Attack();
                break;
            case BossState.MagicBlade:
                CastMagicBlade();
                break;
            case BossState.MagicFire:
                CastMagicFire();
                break;
            case BossState.Death:
                // Death state is handled in Die() method
                break;
        }

        UpdateAnimation();
    }

    void UpdateState(float distanceToPlayer)
    {
        // Không cập nhật trạng thái nếu đang chết hoặc đang chạy animation đặc biệt
        if (currentState == BossState.Death || isPlayingSpecialAnimation)
            return;

        // Nếu đã phát hiện player, luôn đuổi theo và tấn công
        if (playerDetected)
        {
            // Quyết định loại tấn công dựa trên khoảng cách
            if (distanceToPlayer <= 2.0f) // Khoảng cách 2 ô - Chém
            {
                // Chỉ chuyển sang tấn công nếu không đang thực hiện hành động tấn công khác
                if (currentState != BossState.Attack &&
                    currentState != BossState.MagicBlade &&
                    currentState != BossState.MagicFire)
                {
                    enteredAttackRangeTime = Time.time;

                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        currentState = BossState.Attack;
                    }
                    else if (Time.time - lastMagicBladeTime >= magicBladeCooldown)
                    {
                        currentState = BossState.MagicBlade;
                    }
                }
            }
            else if (distanceToPlayer <= 3.0f) // Khoảng cách 3 ô - Phép thuật (Magic Blade)
            {
                if (currentState != BossState.Attack &&
                    currentState != BossState.MagicBlade &&
                    currentState != BossState.MagicFire)
                {
                    if (Time.time - lastMagicBladeTime >= magicBladeCooldown)
                    {
                        currentState = BossState.MagicBlade;
                    }
                    else if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        // Nếu Magic Blade đang hồi, sử dụng tấn công thường
                        currentState = BossState.Attack;
                    }
                }
            }
            else if (distanceToPlayer <= 4.0f) // Khoảng cách 4 ô - Chiêu lửa (Magic Fire)
            {
                if (currentState != BossState.Attack &&
                    currentState != BossState.MagicBlade &&
                    currentState != BossState.MagicFire)
                {
                    if (Time.time - lastMagicFireTime >= magicFireCooldown)
                    {
                        currentState = BossState.MagicFire;
                    }
                    else if (Time.time - lastMagicBladeTime >= magicBladeCooldown)
                    {
                        // Nếu Magic Fire đang hồi, thử dùng Magic Blade
                        currentState = BossState.MagicBlade;
                    }
                    else if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        // Nếu cả hai kỹ năng đều đang hồi, sử dụng tấn công thường
                        currentState = BossState.Attack;
                    }
                }
            }
            else
            {
                // Khoảng cách xa hơn, đuổi theo player
                if (currentState != BossState.MagicBlade &&
                    currentState != BossState.MagicFire &&
                    currentState != BossState.Attack)
                {
                    currentState = BossState.Chase;
                }
            }
        }
        else
        {
            // Chưa phát hiện player, tiếp tục tuần tra
            if (distanceToPlayer <= detectionRange)
            {
                playerDetected = true;
                currentState = BossState.Chase;
            }
            else if (currentState != BossState.MagicBlade &&
                     currentState != BossState.MagicFire &&
                     currentState != BossState.Attack)
            {
                currentState = BossState.Patrol;
            }
        }
    }

    void Idle()
    {
        rb.linearVelocity = Vector2.zero;
    }

    void Patrol()
    {
        if (patrolCenter == null) return;

        Vector2 direction = (patrolTarget - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Flip sprite based on movement direction
        sprite.flipX = direction.x < 0;

        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f)
        {
            if (movingRight)
            {
                patrolTarget = patrolCenter.position + Vector3.left * patrolRange;
            }
            else
            {
                patrolTarget = patrolCenter.position + Vector3.right * patrolRange;
            }
            movingRight = !movingRight;
        }
    }

    void Chase()
    {
        float speed = isAngry ? chaseSpeed * angerSpeedMultiplier : chaseSpeed;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        // Flip sprite based on movement direction
        sprite.flipX = direction.x < 0;
    }

    // Kiểm tra xem có nên di chuyển ra xa để sử dụng phép thuật không
    bool ShouldRepositionForMagic(float distanceToPlayer)
    {
        // Chỉ di chuyển ra xa khi cả hai kỹ năng phép thuật đã sẵn sàng
        bool magicReady = (Time.time - lastMagicFireTime >= magicFireCooldown) ||
                         (Time.time - lastMagicBladeTime >= magicBladeCooldown);

        // Nếu đang ở quá gần và phép thuật đã sẵn sàng, di chuyển ra xa
        return magicReady && distanceToPlayer < 3.0f && Random.value < 0.3f; // 30% cơ hội di chuyển ra xa
    }

    // Di chuyển ra xa để sử dụng phép thuật
    void RepositionForMagicAttack()
    {
        // Tính toán vị trí mới ở khoảng cách thích hợp
        Vector2 direction = ((Vector2)transform.position - (Vector2)player.position).normalized;
        repositionTarget = (Vector2)player.position + direction * preferredMagicDistance;

        // Đánh dấu đang di chuyển đến vị trí mới
        isRepositioning = true;
    }

    // Di chuyển đến vị trí mục tiêu
    void MoveToPosition()
    {
        Vector2 direction = ((Vector2)repositionTarget - (Vector2)transform.position).normalized;
        float speed = isAngry ? chaseSpeed * angerSpeedMultiplier : chaseSpeed;

        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        // Flip sprite based on movement direction
        sprite.flipX = direction.x < 0;
    }

    void Attack()
    {
        // Chỉ dừng di chuyển khi đang thực hiện đòn tấn công
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // Nếu không đang trong animation tấn công, vẫn di chuyển đến player
            float speed = isAngry ? chaseSpeed * angerSpeedMultiplier : chaseSpeed;
            Vector2 moveDirection = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(moveDirection.x * speed * 0.5f, rb.linearVelocity.y); // Di chuyển chậm hơn khi đang chuẩn bị tấn công
        }

        // Face the player
        Vector2 direction = (player.position - transform.position);
        sprite.flipX = direction.x < 0;

        if (Time.time - enteredAttackRangeTime >= attackDelayBeforeFirstHit)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;

                if (animator != null)
                    animator.SetTrigger("Attack");

                // Sau khi tấn công, chạy coroutine để quay lại trạng thái đuổi theo
                StartCoroutine(ReturnToChaseAfterAttack());
            }
        }
    }

    void CastMagicBlade()
    {
        // Nếu đang chạy animation đặc biệt, không thực hiện
        if (isPlayingSpecialAnimation) return;

        // Chỉ dừng di chuyển khi đang thực hiện kỹ năng
        rb.linearVelocity = Vector2.zero;

        // Face the player
        Vector2 direction = (player.position - transform.position);
        sprite.flipX = direction.x < 0;

        if (animator != null)
            animator.SetTrigger("MagicBlade");

        lastMagicBladeTime = Time.time;

        // Đánh dấu đang chạy animation đặc biệt
        isPlayingSpecialAnimation = true;
        StartCoroutine(ReturnToChaseAfterAnimation("Magic_blade"));
    }

    void CastMagicFire()
    {
        // Nếu đang chạy animation đặc biệt, không thực hiện
        if (isPlayingSpecialAnimation) return;

        // Chỉ dừng di chuyển khi đang thực hiện kỹ năng
        rb.linearVelocity = Vector2.zero;

        // Face the player
        Vector2 direction = (player.position - transform.position);
        sprite.flipX = direction.x < 0;

        if (animator != null)
            animator.SetTrigger("MagicFire");

        lastMagicFireTime = Time.time;

        // Đánh dấu đang chạy animation đặc biệt
        isPlayingSpecialAnimation = true;
        StartCoroutine(ReturnToChaseAfterAnimation("Magic_fire"));
    }

    void EnterAngerState()
    {
        // Nếu đã tức giận rồi hoặc đang chạy animation đặc biệt, không thực hiện
        if (isAngry || isPlayingSpecialAnimation) return;

        isAngry = true;

        // Lưu trạng thái hiện tại để quay lại sau khi animation kết thúc
        BossState previousState = currentState;

        // Dừng di chuyển
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Anger");

        // Tăng sức mạnh trong trạng thái tức giận
        attackDamage = Mathf.RoundToInt(attackDamage * angerDamageMultiplier);
        magicBladeDamage = Mathf.RoundToInt(magicBladeDamage * angerDamageMultiplier);
        magicFireDamage = Mathf.RoundToInt(magicFireDamage * angerDamageMultiplier);

        // Giảm thời gian hồi chiêu
        attackCooldown /= angerSpeedMultiplier;
        magicBladeCooldown /= angerSpeedMultiplier;
        magicFireCooldown /= angerSpeedMultiplier;

        // Đánh dấu đang chạy animation đặc biệt
        isPlayingSpecialAnimation = true;

        // Tính thời gian kết thúc animation
        angerAnimationEndTime = Time.time + 1.0f;

        // Sau khi animation kết thúc, chuyển sang trạng thái Chase
        StartCoroutine(DelayedStateChange(BossState.Chase, 1.0f));
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = BossState.Death;
        rb.linearVelocity = Vector2.zero;

        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        if (animator != null)
            animator.SetTrigger("Death");

        // Destroy after death animation (or you could leave the corpse)
        StartCoroutine(DestroyAfterDeathAnimation());
    }

    IEnumerator ReturnToChaseAfterAnimation(string animationName)
    {
        // Wait for animation to complete (approximate time)
        float animationTime = 1.0f;
        switch (animationName)
        {
            case "Magic_blade":
                animationTime = 1.2f;
                break;
            case "Magic_fire":
                animationTime = 1.5f;
                break;
            case "Anger":
                animationTime = 1.0f;
                break;
        }

        yield return new WaitForSeconds(animationTime);

        if (!isDead)
        {
            isPlayingSpecialAnimation = false;
            currentState = BossState.Chase;
        }
    }

    IEnumerator ReturnToChaseAfterAttack()
    {
        // Đợi animation tấn công hoàn thành (khoảng 0.5 giây)
        yield return new WaitForSeconds(0.5f);

        if (!isDead && currentState == BossState.Attack)
        {
            // Sau khi tấn công xong, quay lại trạng thái đuổi theo
            currentState = BossState.Chase;
        }
    }

    IEnumerator DelayedStateChange(BossState newState, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!isDead)
        {
            isPlayingSpecialAnimation = false;
            currentState = newState;
        }
    }

    IEnumerator DestroyAfterDeathAnimation()
    {
        // Wait for death animation to complete
        yield return new WaitForSeconds(2.0f);

        // You can either destroy the object or just disable it
        gameObject.SetActive(false);
        // Uncomment to destroy: Destroy(gameObject);
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            // Set animation parameters based on state
            if (HasParameter("IsMoving", animator))
            {
                animator.SetBool("IsMoving", rb.linearVelocity.magnitude > 0.1f);
            }

            if (HasParameter("State", animator))
            {
                // Chỉ cập nhật State khi không đang chạy animation đặc biệt
                if (!isPlayingSpecialAnimation)
                {
                    // Map trạng thái sang giá trị phù hợp với sơ đồ animation
                    int animState = 0; // Mặc định là Idle (0)

                    if (currentState == BossState.Patrol)
                    {
                        animState = 1; // Walk
                    }
                    else if (currentState == BossState.Chase)
                    {
                        animState = 2; // Run
                    }

                    animator.SetInteger("State", animState);
                }
            }

            // Thêm tham số IsAngry để có thể thay đổi animation khi tức giận
            if (HasParameter("IsAngry", animator))
            {
                animator.SetBool("IsAngry", isAngry);
            }
        }
    }

    bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    // Animation Events - Called from animation frames
    public void OnAttackAnimationEvent()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }
    }

    public void OnMagicBladeAnimationEvent()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= magicBladeRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(magicBladeDamage);
        }
    }

    public void OnMagicFireAnimationEvent()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= magicFireRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(magicFireDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw magic ranges
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, magicBladeRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, magicFireRange);

        // Draw patrol range
        if (patrolCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(patrolCenter.position + Vector3.left * patrolRange,
                            patrolCenter.position + Vector3.right * patrolRange);
        }
    }
}
