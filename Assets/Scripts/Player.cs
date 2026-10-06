using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float speed = 3f;
    public float jumpForce = 5f;
    public float wallJumpHorizontal = 5f;

    private Rigidbody2D rb;

    private bool isGraunded = false;
    private bool isOnWall = false;

    // -1 = parede à direita
    //  1 = parede à esquerda
    private int wallDirection = 0;

    Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");

        // Movimento normal
        rb.linearVelocity = new Vector2(
            moveHorizontal * speed,
            rb.linearVelocity.y
        );

        // Animação
        if (moveHorizontal < 0)
        {
            animator.SetInteger("trans", 1);
            spriteRenderer.flipX = true;
        }
        else if (moveHorizontal > 0)
        {
            animator.SetInteger("trans", 1);
            spriteRenderer.flipX = false;
        }
        else
        {
            animator.SetInteger("trans", 0);
        }

        // Pulo normal
        if (Input.GetKeyDown(KeyCode.Space) && isGraunded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }

        // WALL JUMP
        if (Input.GetKeyDown(KeyCode.Space) && isOnWall && !isGraunded)
        {
            rb.linearVelocity = new Vector2(
                wallDirection * wallJumpHorizontal,
                jumpForce
            );

            isOnWall = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGraunded = true;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            isOnWall = true;

            // Descobre o ponto real onde encostou
            ContactPoint2D contato = collision.GetContact(0);

            // Normal aponta para fora da parede
            if (contato.normal.x > 0)
            {
                // Parede está à esquerda
                wallDirection = 1;
            }
            else if (contato.normal.x < 0)
            {
                // Parede está à direita
                wallDirection = -1;
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isOnWall = true;

            ContactPoint2D contato = collision.GetContact(0);

            if (contato.normal.x > 0)
            {
                wallDirection = 1;
            }
            else if (contato.normal.x < 0)
            {
                wallDirection = -1;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGraunded = false;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            isOnWall = false;
            wallDirection = 0;
        }
    }
}