using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillHUDManager : MonoBehaviour
{
    [Header("UI References")]
    public Image skillIconDisplay;
    public TextMeshProUGUI quantityText;

    [Header("Skill Database")]
    public List<SkillData> availableSkills = new List<SkillData>();
    private int currentSkillIndex = 0;

    void Start()
    {
        UpdateUI();
    }

    // Call this function from Button's OnClick() event
    public void SwitchToNextSkill()
    {
        if (availableSkills.Count == 0) return;

        currentSkillIndex++;
        if (currentSkillIndex >= availableSkills.Count)
        {
            currentSkillIndex = 0; // Loop back to the first skill
        }

        UpdateUI();
    }
public bool UseCurrentSkill()
{
    if (availableSkills.Count == 0) return false;

    SkillData selected = availableSkills[currentSkillIndex];

    if (selected.currentQuantity > 0)
    {
        selected.currentQuantity--;
        UpdateUI();
        return true;
    }

    return false; // Out of ammo!
}

public SkillData GetCurrentSkill()
{
    return availableSkills[currentSkillIndex];
}

   public void UpdateUI()
{
    if (availableSkills.Count > 0 && availableSkills[currentSkillIndex] != null)
    {
        SkillData selected = availableSkills[currentSkillIndex];

        // Safety checks to tell you exactly what is missing
        if (skillIconDisplay != null) 
            skillIconDisplay.sprite = selected.icon;
        else 
            Debug.LogError("Assign the 'Skill Icon Display' in the Inspector!");

        if (quantityText != null) 
            quantityText.text = $"{selected.currentQuantity:00}/{selected.maxQuantity:00}";
        else 
            Debug.LogError("Assign the 'Quantity Text' in the Inspector!");
    }
}
}