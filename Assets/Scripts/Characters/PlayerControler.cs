using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpCount;
    public int maxJumps = 2;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isAttacking;
    private Vector3 startPosition;
    public GameObject GameOver;

    // Thêm thông số tấn công
    public float attackRange = 1f;
    public int attackDamage = 1;

    private static PlayerController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
    }

    void Update()
    {
        Move();
        Jump();
        Attack();
        UpdateAnimation();
    }

    void Move()
    {
        if (isAttacking) return;

        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (moveInput > 0)
            spriteRenderer.flipX = false;
        else if (moveInput < 0)
            spriteRenderer.flipX = true;

        animator.SetBool("isRunning", moveInput != 0);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;

            if (jumpCount == 1)
                animator.SetTrigger("Jump1");
            else if (jumpCount == 2)
                animator.SetTrigger("Jump2");
        }
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("Attack");

            // Gây sát thương cho quái
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    Debug.Log("Enemy hit! Damage: " + attackDamage);
                    hit.GetComponent<EnemyHealth>().TakeDamage(attackDamage);
                }
            }

            Invoke("ResetAttack", 0.5f);
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    void UpdateAnimation()
    {
        if (isGrounded)
        {
            animator.SetBool("isIdle", rb.linearVelocity.x == 0);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
            animator.SetBool("isGrounded", true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ResetState()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        isGrounded = true;
        jumpCount = 0;
        isAttacking = false;

        animator.SetBool("isRunning", false);
        animator.SetBool("isGrounded", true);
        animator.SetBool("isIdle", true);

        animator.ResetTrigger("Jump1");
        animator.ResetTrigger("Jump2");
        animator.ResetTrigger("Attack");

        animator.Play("Player_Idle");

        transform.position = startPosition;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}