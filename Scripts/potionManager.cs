using UnityEngine;

public class PotionManager : MonoBehaviour
{
    public static PotionManager instance;

    [Header("Potion Data")]
    public int maxPotions = 3;
    public int currentPotion;

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

    void Start()
    {
        currentPotion = maxPotions;
    }

    void Update()
    {
    
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryUsePotion();
        }
    }

public void TryUsePotion()
{
    //  Check if we have potions
    if (currentPotion <= 0) 
        return;

    // Check if player health controller exists
    if (HealthManager.instance == null) 
        return;
    
    //  Check if player is NOT already at max health
    if (HealthManager.instance.currentHealth < HealthManager.instance.maxHealth)
    {
        
        // Heal the player
        HealthManager.instance.Heal(1);
        
        currentPotion--; 
        
        Debug.Log("Potion Used. Remaining: " + currentPotion);
    }
    else
    {
        //any visual effect that the health is full already? maybe?
        Debug.Log("Health is already full. Potion not used.");
    }
}
    
    // Call this when you pick up a potion item
    public void AddPotion()
    {
        if(currentPotion < maxPotions)
        {
            currentPotion++;
        }
    }
}