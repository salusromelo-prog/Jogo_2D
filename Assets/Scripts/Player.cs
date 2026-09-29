using UnityEngine;
using UnityEngine.SceneManagement;
public class Player : MonoBehaviour
{

    public float speed = 3f;

    private Rigidbody2D rb;

    private bool isGraunded = false;

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
        float moveHorizontal = Input.GetAxis("Horizontal"); //Vai reconhecer o movimento horizontal

        rb.linearVelocity = new Vector2(moveHorizontal * speed, rb.linearVelocity.y); //Vai aplicar a velocidade horizontal ao Rigidbody2D

        if(moveHorizontal < 0)
        {
         animator.SetInteger("trans", 1);
            spriteRenderer.flipX = true; // Vira para a esquerda


        }
        else if(moveHorizontal > 0)

        {
            animator.SetInteger("trans", 1);
              spriteRenderer.flipX = false; // Vira para a esquerda


        }
        else
        {
            animator.SetInteger("trans", 0);

        }

        if (Input.GetKeyDown(KeyCode.Space) && isGraunded)
        {
            rb.AddForce(new Vector2(0f, 5f), ForceMode2D.Impulse); //Vai aplicar uma força vertical ao Rigidbody2D


        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGraunded = true;
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")){
            isGraunded = false;
        }


    }

}
