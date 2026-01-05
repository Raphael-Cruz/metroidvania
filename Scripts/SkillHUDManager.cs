using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillHUDManager : MonoBehaviour
{
    [Header("UI References")]
    public Image skillIconDisplay;
    public TextMeshProUGUI quantityText;
    public CanvasGroup hudCanvasGroup; 

    [Header("Skill Database")]
    public List<SkillData> availableSkills = new List<SkillData>();
    private int currentSkillIndex = 0;

    void Start()
    {
        // Auto-grab CanvasGroup if not assigned
        if (hudCanvasGroup == null) 
            hudCanvasGroup = GetComponent<CanvasGroup>();

        UpdateUI();
    }

    public void SwitchToNextSkill()
    {
        if (availableSkills.Count <= 1) return;

        currentSkillIndex++;
        if (currentSkillIndex >= availableSkills.Count)
        {
            currentSkillIndex = 0;
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

        return false; 
    }

public void UpdateUI()
{
 
    if (availableSkills.Count == 0 || currentSkillIndex >= availableSkills.Count || availableSkills[currentSkillIndex] == null)
    {
        // Hide the HUD using the Canvas Group
        if (hudCanvasGroup != null) hudCanvasGroup.alpha = 0f;

   
        if (quantityText != null) 
            quantityText.text = "00/00";
           
            
        // Clear the icon sprite
        if (skillIconDisplay != null)
            skillIconDisplay.sprite = null;


        return; 

        
    }

    //  Handle the "Active Skill" state
    if (hudCanvasGroup != null) hudCanvasGroup.alpha = 1f;

    SkillData selected = availableSkills[currentSkillIndex];

    if (skillIconDisplay != null) 
    {
        skillIconDisplay.sprite = selected.icon;
        // Make sure icon is opaque
        Color c = skillIconDisplay.color;
        c.a = 1f;
        skillIconDisplay.color = c;
    }

    if (quantityText != null) 
        quantityText.text = $"{selected.currentQuantity:00}/{selected.maxQuantity:00}";
       
        if (selected.currentQuantity <= 1)
        {
            quantityText.color = Color.red; // Visual warning
        }
        else
        {
            quantityText.color = Color.white; // Normal state
        }
}
    // Call this whenever you pick up a new skill to refresh the HUD
    public void AddSkill(SkillData newSkill)
    {
        if (!availableSkills.Contains(newSkill))
        {
            availableSkills.Add(newSkill);
            UpdateUI();
        }
    }
}