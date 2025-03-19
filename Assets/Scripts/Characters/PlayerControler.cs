using UnityEngine;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
        Jump();
        UpdateAnimation();
    }

    void Move()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (moveInput > 0)
            spriteRenderer.flipX = false; // Hướng sang phải
        else if (moveInput < 0)
            spriteRenderer.flipX = true; // Hướng sang trái

        animator.SetBool("isRunning", moveInput != 0);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;

            if (jumpCount == 1)
                animator.SetTrigger("Jump1"); // Hoạt ảnh nhảy lần 1
            else if (jumpCount == 2)
                animator.SetTrigger("Jump2"); // Hoạt ảnh nhảy lần 2
        }
    }

    void UpdateAnimation()
    {
        if (isGrounded)
        {
            if (rb.linearVelocity.x == 0)
                animator.SetBool("isIdle", true);
            else
                animator.SetBool("isIdle", false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0; // Reset số lần nhảy khi chạm đất
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
}
