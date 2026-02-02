using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BossHealthUI : MonoBehaviour
{
    [Header("References")]
    public EnemyHealthController bossHealth;
    [Tooltip("The main yellow slider representing current HP")]
    public Slider healthSlider; 
    [Tooltip("The red background slider representing delayed damage")]
    public Slider easeSlider;
    public TextMeshProUGUI bossNameText;

    [Header("Settings")]
    public string bossName = "Cyber Pig";
    public float easeSpeed = 2f;

    private int maxHealth;

    private void Start()
    {
        // 1. Try to find the boss if not assigned
        if (bossHealth == null)
        {
            GameObject boss = GameObject.FindGameObjectWithTag("Boss");
            if (boss != null)
            {
                bossHealth = boss.GetComponent<EnemyHealthController>();
            }
            
            // Fallback: Try finding CyberPig directly
            if (bossHealth == null)
            {
                var pig = FindObjectOfType<CyberPig>();
                if (pig != null) bossHealth = pig.GetComponent<EnemyHealthController>();
            }
        }

        // 2. Set Name
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }

        // 3. Initialize Health Values
        if (bossHealth != null)
        {
            // Determine Max Health
            // Default to current (in case regular enemy)
            maxHealth = bossHealth.totalhealth; 

            // If it's the CyberPig, use the specific MaxHealth property for accuracy
            CyberPig pig = bossHealth.GetComponent<CyberPig>();
            if (pig != null)
            {
                maxHealth = pig.MaxHealth;
            }
            
            InitSliders(maxHealth);
            
            // Subscribe to damage event
            bossHealth.onDamageCallback += OnDamage;
        }
    }

    private void Update()
    {
        if (bossHealth == null) return;

        // Main Slider: Snap to current health
        if (healthSlider.value != bossHealth.totalhealth)
        {
            healthSlider.value = bossHealth.totalhealth;
        }

        // Ease Slider: Lerp to current health
        if (easeSlider.value != healthSlider.value)
        {
            easeSlider.value = Mathf.Lerp(easeSlider.value, healthSlider.value, Time.deltaTime * easeSpeed);
            
            // Snap if very close
            if (Mathf.Abs(easeSlider.value - healthSlider.value) < 0.05f)
            {
                easeSlider.value = healthSlider.value;
            }
        }
    }

    private void InitSliders(int max)
    {
        maxHealth = max;
        if (healthSlider)
        {
            healthSlider.maxValue = max;
            healthSlider.value = bossHealth.totalhealth; // Start at current
        }
        if (easeSlider)
        {
            easeSlider.maxValue = max;
            easeSlider.value = bossHealth.totalhealth; // Start at current
        }
    }

    private void OnDamage(int damage)
    {
        // The Update loop handles the slider updates.
        // We could trigger a flash on the bar here if we wanted extra feedback.
    }
    
    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.onDamageCallback -= OnDamage;
        }
    }
}
