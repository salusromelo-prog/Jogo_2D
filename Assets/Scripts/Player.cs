using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 6f;        // velocidade andando
    public float jumpForce = 10f;   // força do pulo

    [Header("Parede")]
    public float wallSlideSpeed = 2f;   // velocidade máxima de queda quando está deslizando na parede
    public float wallJumpX = 7f;        // força para o lado no pulo da parede
    public float wallJumpTime = 0.2f;   // tempo sem controle depois do pulo da parede (para sair da parede)

    [Header("Detecção")]
    public float checkDistance = 0.1f;  // distância para sentir chão e parede

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private ContactFilter2D filtro;                           // regras da detecção
    private RaycastHit2D[] resultados = new RaycastHit2D[5];  // guarda o que a detecção encontrou

    private bool noChao;        // true quando está pisando no chão
    private int paredeLado;     // 1 = parede na direita, -1 = parede na esquerda, 0 = sem parede
    private float travaTimer;   // tempo restante sem controle (depois do pulo da parede)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // Ignora triggers (como a área da bala), só queremos sentir coisas sólidas
        filtro = new ContactFilter2D();
        filtro.useTriggers = false;
    }

    void Update()
    {
        // Jogo parado (menu ou Game Over): não faz nada
        if (Time.timeScale == 0)
        {
            return;
        }

        float moveHorizontal = Input.GetAxis("Horizontal");

        // 1) Descobre onde o player está encostando
        VerificarChaoEParede();

        // 2) Andar (só se não estiver na pausa do pulo da parede)
        if (travaTimer > 0)
        {
            travaTimer -= Time.deltaTime;
        }
        else
        {
            rb.linearVelocity = new Vector2(moveHorizontal * speed, rb.linearVelocity.y);

            if (moveHorizontal < 0)
            {
                animator.SetInteger("trans", 1);
                spriteRenderer.flipX = true;    // olha para a esquerda
            }
            else if (moveHorizontal > 0)
            {
                animator.SetInteger("trans", 1);
                spriteRenderer.flipX = false;   // olha para a direita
            }
            else
            {
                animator.SetInteger("trans", 0);
            }
        }

        // 3) Pular
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (noChao)
            {
                // Pulo normal: troca só a velocidade vertical
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                GameManager.Tocar("pulo");
            }
            else if (paredeLado != 0)
            {
                // Pulo da parede: vai para o lado oposto da parede e para cima
                rb.linearVelocity = new Vector2(-paredeLado * wallJumpX, jumpForce);
                GameManager.Tocar("pulo");
                travaTimer = wallJumpTime;

                // Vira o personagem para longe da parede
                spriteRenderer.flipX = (paredeLado > 0);
            }
        }

        // 4) Pulo curto: soltou o espaço enquanto sobe, corta a subida pela metade
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // 5) Deslizar na parede: está no ar, empurrando contra a parede e caindo
        bool empurrandoParede = (paredeLado != 0) && (moveHorizontal * paredeLado > 0);

        if (!noChao && empurrandoParede && rb.linearVelocity.y < -wallSlideSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
    }

    // Atualiza as variáveis noChao e paredeLado
    void VerificarChaoEParede()
    {
        // Só conta como chão se não estiver subindo (evita "chão" logo depois de pular)
        noChao = Tocando(Vector2.down) && rb.linearVelocity.y <= 0.1f;

        paredeLado = 0;

        // Parede só importa quando está no ar
        if (!noChao)
        {
            if (Tocando(Vector2.right))
            {
                paredeLado = 1;
            }
            else if (Tocando(Vector2.left))
            {
                paredeLado = -1;
            }
        }
    }

    // Empurra o collider um pouquinho na direção pedida e vê se bateu em chão ou parede
    bool Tocando(Vector2 direcao)
    {
        int quantidade = col.Cast(direcao, filtro, resultados, checkDistance);

        for (int i = 0; i < quantidade; i++)
        {
            GameObject obj = resultados[i].collider.gameObject;

            // Só vale o que tem a tag Ground (chão) ou Wall (parede)
            if (obj.CompareTag("Ground") || obj.CompareTag("Wall"))
            {
                return true;
            }
        }

        return false;
    }
}
