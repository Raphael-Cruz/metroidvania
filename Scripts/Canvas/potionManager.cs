using UnityEngine;
using System.Collections;

public class PotionManager : MonoBehaviour
{
    public static PotionManager instance;


    [Header("Potion Data")]
    public int maxPotions = 3;
    public int currentPotion;
    public Animator anim;

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
    
        if (InputManager.instance.GetPotionDown())
        {
            TryUsePotion();
        }
    }
public void TryUsePotion()
{
    if (currentPotion <= 0) return;

    // Safety Check for Singletons
    if (HealthManager.instance == null || PlayerMovement.instance == null)
    {
        Debug.LogWarning("Managers not found in this scene!");
        return;
    }

    // Refresh Animator if it was destroyed by a scene change
    if (anim == null)
    {
        // This looks for the Animator on the Player object
        anim = PlayerMovement.instance.visual.GetComponent<Animator>();
    }

    
if (HealthManager.instance.currentHealth < HealthManager.instance.maxHealth && PlayerMovement.instance.IsOnGround)
    {
        // Start the Coroutine to handle the pause
        StartCoroutine(DrinkPotionRoutine());
    }
    else if (!PlayerMovement.instance.IsOnGround)
    {
        Debug.Log("You must be on the ground to drink a potion!");
    }
    else
    {
        Debug.Log("Health is already full.");
    }
}
private IEnumerator DrinkPotionRoutine()
{
    // Stop Movement Logic
    PlayerMovement.instance.canMove = false;
    
    // Stop Physics (Stops sliding/falling)
    PlayerMovement.instance.theRB.velocity = Vector2.zero;
    PlayerMovement.instance.theRB.constraints = RigidbodyConstraints2D.FreezeAll;

    // Play Animation
    if (anim == null) anim = PlayerMovement.instance.visual.GetComponent<Animator>();
    anim.SetTrigger("Heal");

    HealthManager.instance.Heal(1);
    currentPotion--;

    // Wait for animation (match your clip length)
    yield return new WaitForSeconds(0.8f);

    // Release
    PlayerMovement.instance.theRB.constraints = RigidbodyConstraints2D.FreezeRotation;
   
    PlayerMovement.instance.canMove = true;
}


    // Call this when you pick up a potion item but right now the only way to refresh potion is on savepoint
    public void AddPotion()
    {
        if(currentPotion < maxPotions)
        {
            currentPotion++;
        }
    }
}