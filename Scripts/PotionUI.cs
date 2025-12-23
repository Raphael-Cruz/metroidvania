using UnityEngine;
using UnityEngine.UI;

public class PotionUI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] potionSlots; // Drag your 3 Image slots here
    public Sprite fullPotionSprite;
    public Sprite emptyPotionSprite;

    void Update()
    {
        // Safety check: Ensure the Manager exists
        if (PotionManager.instance == null) return;

        // Loop through the UI slots
        for (int i = 0; i < potionSlots.Length; i++)
        {
            // If the slot is within the limit of max potions logic
            if (i < PotionManager.instance.maxPotions)
            {
                potionSlots[i].enabled = true;

                // Ask the Manager: "Is this slot full?"
                if (i < PotionManager.instance.currentPotion)
                {
                    potionSlots[i].sprite = fullPotionSprite;
                }
                else
                {
                    potionSlots[i].sprite = emptyPotionSprite;
                }
            }
            else
            {
                // Hide extra slots if we have more UI images than max potions
                potionSlots[i].enabled = false;
            }
        }
    }
}