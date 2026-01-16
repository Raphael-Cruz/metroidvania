using UnityEngine;

public class BossSkillCaster : MonoBehaviour
{
    [Header("Skill Prefab")]
    [SerializeField] private GameObject circleSkillPrefab;
    
    [Header("Casting Settings")]
    [SerializeField] private float startDelay = 2f; // Delay before the first cast
    [SerializeField] private float minCastInterval = 3f;
    [SerializeField] private float maxCastInterval = 6f;
    [SerializeField] private Transform skillSpawnPoint; // Where the skill spawns from
    
    [Header("Optional: Boss Animator")]
    [SerializeField] private Animator bossAnimator;
    
    private float nextCastTime;
    private bool canCast = true;
    
    private void Start()
    {
        // Schedule the first cast with initial delay
        nextCastTime = Time.time + startDelay;
    }
    
    private void Update()
    {
        if (canCast && Time.time >= nextCastTime)
        {
            CastCircleSkill();
            ScheduleNextCast();
        }
    }
    
private void CastCircleSkill()
{
    if (circleSkillPrefab == null) return;

    Vector3 spawnPosition = skillSpawnPoint != null ? skillSpawnPoint.position : transform.position;
    
    // Cria o objeto
    GameObject skill = Instantiate(circleSkillPrefab, spawnPosition, Quaternion.identity);
    
    // ATIVA o objeto (isso resolve o erro da Coroutine)
    skill.SetActive(true);
    
    CircleSkillBehavior behavior = skill.GetComponent<CircleSkillBehavior>();
    if (behavior != null)
    {
        GameObject player = GameObject.FindWithTag("Player");
        Vector3 targetPos = player != null ? player.transform.position : transform.position + transform.right;
        
        behavior.Initialize(gameObject, targetPos);
    }
}

public void FlipSpawnPoint(bool facingRight)
{
    if (skillSpawnPoint != null)
    {
        // Se você usa o método de inverter a posição local:
        Vector3 localPos = skillSpawnPoint.localPosition;
        
        // Garante que o X seja positivo se estiver para a direita, e negativo para a esquerda
        float posX = Mathf.Abs(localPos.x);
        localPos.x = facingRight ? posX : -posX;
        
        skillSpawnPoint.localPosition = localPos;
    }
}
    
    private void ScheduleNextCast()
    {
        // Random interval between min and max
        float randomInterval = Random.Range(minCastInterval, maxCastInterval);
        nextCastTime = Time.time + randomInterval;
    }
    
    // Call this method to enable/disable skill casting (e.g., during boss phases)
    public void SetCanCast(bool enabled)
    {
        canCast = enabled;
    }
}
