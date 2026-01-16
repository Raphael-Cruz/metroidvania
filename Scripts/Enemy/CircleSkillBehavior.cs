using System.Collections;
using UnityEngine;

public class CircleSkillBehavior : MonoBehaviour
{
    [Header("Configurações de Velocidade")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float castingDuration = 1.2f; 
    [SerializeField] private float destroyAnimationDuration = 0.5f;
    
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveDirection;
    private bool isFired = false;
    private bool isDestroying = false;
    private GameObject caster;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if(rb != null) rb.gravityScale = 0; 
    }

    private void OnEnable()
{
    // Garante que o Animator resete quando o objeto for ativado
    if (animator != null)
    {
        animator.Rebind();
        animator.Update(0f);
    }
}

    public void Initialize(GameObject casterObject, Vector3 targetPosition)
    {
        caster = casterObject;
        // Calcula a direção inicial
        moveDirection = (targetPosition - transform.position).normalized;
        
        // Inicia a sequência
        StartCoroutine(SkillSequence());
    }

    IEnumerator SkillSequence()
    {
        // Fica parado durante o cast
        isFired = false;
        
        // Tenta dar play explicitamente na animação
        if (animator != null) animator.Play("Casting"); 

        // Aguarda o tempo de cast
        yield return new WaitForSeconds(castingDuration);

        // --- DISPARO ---
        isFired = true;
        
        // Se houver uma animação de "Fired", forçamos ela aqui
        if (animator != null) animator.Play("Fired");

        // Destruição de segurança após 5 segundos (caso saia do mapa)
        Destroy(gameObject, 5f); 
    }

    void FixedUpdate()
    {
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        if (isFired && !isDestroying)
        {
            // Move usando MovePosition para garantir que a física acompanhe
            Vector2 nextPos = (Vector2)transform.position + (moveDirection * moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(nextPos);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroying) return;

        // Ignora o boss e outras skills
        if (collision.gameObject == caster || collision.CompareTag("Skill") ) return;
        
        // Se bater em qualquer outra coisa (Chão, Parede, Player)
        //no momento o tiro esta quebrando a skill isso nao e desejavel
        StartCoroutine(DestroySequence());
    }

    IEnumerator DestroySequence()
    {
        isDestroying = true;
        isFired = false;
        
        if (animator != null) animator.Play("Destroying");
        
        yield return new WaitForSeconds(destroyAnimationDuration);
        Destroy(gameObject);
    }


}