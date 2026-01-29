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
    private bool canCast = false;
    
    private void Start()
    {
        // Schedule the first cast with initial delay
        nextCastTime = Time.time + startDelay;
    }
    
    
    // Removed Update loop casting logic to allow direct control by CyberPig
    private void Update()
    {
    }
    
    public GameObject CastCircleSkill()
    {
        if (circleSkillPrefab == null) return null;
    
        Vector3 spawnPosition = skillSpawnPoint != null ? skillSpawnPoint.position : transform.position;
        
        // Create the object
        GameObject skill = Instantiate(circleSkillPrefab, spawnPosition, Quaternion.identity);
        
        skill.SetActive(true);
        
        CircleSkillBehavior behavior = skill.GetComponent<CircleSkillBehavior>();
        if (behavior != null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector3 targetPos = player != null ? player.transform.position : transform.position + transform.right;
            
            behavior.Initialize(gameObject, targetPos);
        }
        
        return skill;
    }

public void FlipSpawnPoint(bool facingRight)
{
    if (skillSpawnPoint != null)
    {
        // usa o método de inverter a posição local:
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
    
    // Call this method to enable/disable skill casting (during boss phases)
    public void SetCanCast(bool enabled)
    {
        canCast = enabled;
    }
}
