using System.Collections;
using UnityEngine;

public class CarcassHarvester : MonoBehaviour
{
    [Header("Detection")]
    public Vector3 attackBoxSize = new Vector3(10f, 4f, 10f);
    public Transform detectionCenter; // se null, usa transform do inimigo

    [Header("References")]
    public Transform spawnPoint;          // ponta do cetro (onde a bola aparece)
    public Transform player;

    [Header("Projectile Arc")]
    public float arcHeight = 5f;          // altura máxima do arco parabolico
    public float travelTime = 1.5f;       // tempo de voo até o alvo

    [Header("Timing")]
    public float delayBeforeLaunch = 2f;  // bola fica parada antes de partir
    public float delayBetweenBalls = 1f;  // espera entre destruição e próxima bola
    public int totalBalls = 3;

    [Header("Animation")]
    public Animator animator;

    private bool isAttacking = false;
    private GameObject carcassBallTemplate;

    public bool carcassIsAlive = true;

    public GameObject EnergyWind;


    public static CarcassHarvester instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------
    void Start()
    {
        Transform child = transform.Find("CarcassBall");
        if (child != null)
        {
            carcassBallTemplate = child.gameObject;
            carcassBallTemplate.SetActive(false); // esconde o original
        }
        else
        {
            Debug.LogWarning("CarcassHarvester: filho 'CarcassBall' não encontrado!");
        }

        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        if (detectionCenter == null) detectionCenter = transform;
        if (animator == null) animator = GetComponent<Animator>();

     EnemyHealthController healthController = GetComponent<EnemyHealthController>();
if (healthController != null)
    healthController.onDeathCallback = (hc) => OnDeath();
    }

    // -------------------------------------------------------
    void Update()
    {
        if (!isAttacking && IsPlayerInBox())
            StartCoroutine(AttackSequence());
    }

    // -------------------------------------------------------
    // Detecção por Box (orientada ao transform do inimigo)
    // -------------------------------------------------------
    bool IsPlayerInBox()
    {
        if (player == null) return false;
        Vector3 local = detectionCenter.InverseTransformPoint(player.position);
        return Mathf.Abs(local.x) <= attackBoxSize.x * 0.5f &&
               Mathf.Abs(local.y) <= attackBoxSize.y * 0.5f &&
               Mathf.Abs(local.z) <= attackBoxSize.z * 0.5f;
    }

    // -------------------------------------------------------
    // Sequência completa de ataque
    // -------------------------------------------------------
    IEnumerator AttackSequence()
    {
        isAttacking = true;

        // Idle → Attacking (trigger dispara a transição)
        if (animator != null) animator.SetTrigger("isAttacking");

        // Aguarda o clip de Attacking terminar antes de entrar em Enchant
        yield return new WaitForSeconds(GetTriggerAnimLength("Attacking"));

        // Entra em loop Enchant via boolean
        if (animator != null) animator.SetBool("isEnchanting", true);

        for (int i = 0; i < totalBalls; i++)
        {
            yield return StartCoroutine(LaunchBall());

            if (i < totalBalls - 1)
                yield return new WaitForSeconds(delayBetweenBalls);
        }

        // Ataque encerrado → desliga Enchant, volta pro Idle
        if (animator != null) animator.SetBool("isEnchanting", false);
        isAttacking = false;
    }

    // -------------------------------------------------------
    // Spawn + delay + voo em arco de uma bola
    // -------------------------------------------------------
    IEnumerator LaunchBall()
    {
        if (carcassBallTemplate == null || player == null) yield break;

        // Captura posição do player agora (última posição conhecida)
        Vector3 target = player.position;

        // Instancia a bola no spawnPoint
        Vector3 origin = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up * 2f;
        GameObject ball = Instantiate(carcassBallTemplate, origin, Quaternion.identity);
        ball.SetActive(true);

        // Fica parada por delayBeforeLaunch
        yield return new WaitForSeconds(delayBeforeLaunch);

        // Voa em arco parabolico
        yield return StartCoroutine(ArcFlight(ball, origin, target, travelTime));

        // Destrói ao chegar
        if (ball != null) Destroy(ball);
    }

    // -------------------------------------------------------
    // Movimento parabolico (arco como na ilustração)
    // -------------------------------------------------------
    IEnumerator ArcFlight(GameObject obj, Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (obj == null) yield break;

            float t = Mathf.Clamp01(elapsed / duration);

            // Posição linear
            Vector3 pos = Vector3.Lerp(start, end, t);

            // Componente Y do arco: parábola 4h·t·(1-t)
            pos.y += arcHeight * 4f * t * (1f - t);

            obj.transform.position = pos;

            // Aponta na direção do próximo frame para dar sensação de trajetória
            float tNext = Mathf.Clamp01((elapsed + Time.deltaTime) / duration);
            Vector3 nextPos = Vector3.Lerp(start, end, tNext);
            nextPos.y += arcHeight * 4f * tNext * (1f - tNext);
            Vector3 dir = nextPos - pos;
            if (dir.sqrMagnitude > 0.001f)
                obj.transform.rotation = Quaternion.LookRotation(dir);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (obj != null) obj.transform.position = end;
    }

    // -------------------------------------------------------
    // Helpers de animação
    // -------------------------------------------------------
    float GetTriggerAnimLength(string clipName)
    {
        if (animator == null) return 0.5f;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
            if (clip.name == clipName) return clip.length;
        return 0.5f;
    }

    // -------------------------------------------------------
    // Gizmo do Box de detecção (visível no editor)
    // -------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        Transform center = detectionCenter != null ? detectionCenter : transform;
        Gizmos.matrix = Matrix4x4.TRS(center.position, center.rotation, Vector3.one);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.25f);
        Gizmos.DrawCube(Vector3.zero, attackBoxSize);
        Gizmos.color = new Color(1f, 0.4f, 0f, 1f);
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);
    }

void OnDeath()
{
    carcassIsAlive = false;
    EnergyWind.SetActive(false);

   
    foreach (var entangler in FindObjectsByType<Entangler>(FindObjectsSortMode.None))
    {
        EnemyHealthController hc = entangler.GetComponent<EnemyHealthController>();
        if (hc != null)
            hc.isInvulnerable = false;
    }

  GetComponent<EnemyHealthController>().PerformDefaultDeath();
}
}