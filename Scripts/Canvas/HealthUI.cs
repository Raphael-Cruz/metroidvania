using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Wing Slider References")]
    public Slider leftSlider;
    public Slider rightSlider;

    [Header("Slots Per Wing")]
    public int leftSlots = 4;
    public int rightSlots = 4;

    [Header("Health Flash Reference")]
    public HealthBarFlashing flash; // Drag the component holding your flashing logic

private void Awake()
{
    // The Manager may not be fully initialized yet, but this is the earliest we can try to register.
    if (HealthManager.instance != null && flash != null)
    {
        // Register the UI reference with the  manager
        HealthManager.instance.currentUIFlash = flash;
    }
}

void Start()
{
    // Setup max values based on the UI design (4+4)
    leftSlider.maxValue = leftSlots;
    rightSlider.maxValue = rightSlots;

    // Perform the initial update
    UpdateHealthDisplay();
}
    
    void Update()
    {
        
        UpdateHealthDisplay();
    }

    public void UpdateHealthDisplay()
    {
        if (HealthManager.instance == null) return;
        
        int current = HealthManager.instance.currentHealth;

        // Calculate slider values based on the UI design (4 left, remaining right)
        leftSlider.value = Mathf.Clamp(current, 0, leftSlots);
        rightSlider.value = Mathf.Clamp(current - leftSlots, 0, rightSlots);
        
        // Ensure the flash UI starts in the correct low health state
        if (flash != null)
        {
             flash.SetLowHealthState(current == 1);
        }
    }
}