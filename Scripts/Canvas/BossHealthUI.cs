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
    public static BossHealthUI instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Removed DontDestroyOnLoad if it's already on a persistent canvas root, 
            // but if this script itself is checking, ensure it's not destroyed if that's the intent.
            // If the user said "on my canvas DOndestroy on load", the script is already safe.
        }
        else
        {
            // If we reload scenes and this is duplicated, handle it.
            // But if it's persistent, we keep the first one.
            if (instance != this) Destroy(gameObject);
        }
    }

    private int maxHealth;

    private void Start()
    {
        // 1. Try to find the boss if not assigned
        if (bossHealth == null)
        {
            // Fix: Don't use FindGameObjectWithTag("Boss") if the tag isn't defined in the project, as it crashes.
            // Instead, look for the specific script directly.
            var pig = FindObjectOfType<CyberPig>();
            if (pig != null) bossHealth = pig.GetComponent<EnemyHealthController>();
            
            // Fallback: Try generic enemy health if specific pig not found (less likely but safe)
            if (bossHealth == null)
            {
               var genericBoss = FindObjectOfType<EnemyHealthController>();
               // Only take it if it looks like a boss? For now, leave null if not sure to avoid grabbing random enemies.
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

        // AUTO-HIDE: allowing the object to be Enabled in Inspector (for Awake/Singleton) but hidden on play.
        gameObject.SetActive(false);
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

    public void SetBoss(EnemyHealthController newBoss)
    {
        bossHealth = newBoss;
        
        // Re-subscribe to events
        if (bossHealth != null)
        {
            bossHealth.onDamageCallback -= OnDamage; // Unsubscribe first to avoid duplicates
            bossHealth.onDamageCallback += OnDamage;
            
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        // 1. Try to recover lost reference if null
        if (bossHealth == null)
        {
            GameObject boss = GameObject.FindGameObjectWithTag("Boss");
            if (boss != null) bossHealth = boss.GetComponent<EnemyHealthController>();
            
            if (bossHealth == null)
            {
                var pig = FindObjectOfType<CyberPig>();
                if (pig != null) bossHealth = pig.GetComponent<EnemyHealthController>();
            }
        }

        if (bossHealth == null) return;
        
        // Re-fetch max health
        CyberPig pigComponent = bossHealth.GetComponent<CyberPig>();
        if (pigComponent != null)
        {
            maxHealth = pigComponent.MaxHealth;
        }
        else
        {
            maxHealth = bossHealth.totalhealth; 
        }

        if (healthSlider)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = bossHealth.totalhealth;
        }
        if (easeSlider)
        {
            easeSlider.maxValue = maxHealth;
            easeSlider.value = bossHealth.totalhealth;
        }
    }
    
    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.onDamageCallback -= OnDamage;
        }
    }
}
