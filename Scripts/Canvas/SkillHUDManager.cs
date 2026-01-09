using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SkillHUDManager : MonoBehaviour
{
    [Header("UI")]
    public Image skillIconDisplay;
    public TextMeshProUGUI quantityText;
    public CanvasGroup hudCanvasGroup;

    [Header("All Skills Database")]
    public List<SkillData> allPossibleSkills;

    private readonly List<SkillData> activeSkills = new();
    private int currentSkillIndex = 0;

    private void Awake()
    {
        HideHUD();
    }

    private void Start()
    {
        // Delay one frame so WorldState is guaranteed loaded
        StartCoroutine(DelayedRebuild());
    }

    private IEnumerator DelayedRebuild()
    {
        yield return null;
        RebuildFromWorldState();
    }

    public void RebuildFromWorldState()
    {
        activeSkills.Clear();

        foreach (SkillData skill in allPossibleSkills)
        {
            if (skill == null) continue;

            if (WorldState.UnlockedSkills.Contains(skill.skillID))
            {
                skill.isCollected = true;

                // If loading from save, restore ammo
                if (skill.currentQuantity <= 0)
                    skill.currentQuantity = skill.maxQuantity;

                activeSkills.Add(skill);
            }
        }

        currentSkillIndex = Mathf.Clamp(currentSkillIndex, 0, activeSkills.Count - 1);
        UpdateUI();
    }

    public void AddSkill(SkillData skill)
    {
        if (skill == null) return;

        if (!activeSkills.Contains(skill))
        {
            skill.isCollected = true;
            skill.currentQuantity = skill.maxQuantity;

            activeSkills.Add(skill);
            currentSkillIndex = activeSkills.Count - 1;
            UpdateUI();
        }
    }

    public bool UseCurrentSkill()
    {
        if (activeSkills.Count == 0) return false;

        SkillData skill = activeSkills[currentSkillIndex];
        if (skill.currentQuantity > 0)
        {
            skill.currentQuantity--;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void UpdateUI()
    {
        if (activeSkills.Count == 0)
        {
            HideHUD();
            return;
        }

        hudCanvasGroup.alpha = 1f;
        hudCanvasGroup.blocksRaycasts = true;

        SkillData skill = activeSkills[currentSkillIndex];
        skillIconDisplay.sprite = skill.icon;
        quantityText.text = $"{skill.currentQuantity:00}/{skill.maxQuantity:00}";
    }

    private void HideHUD()
    {
        hudCanvasGroup.alpha = 0f;
        hudCanvasGroup.blocksRaycasts = false;

        if (skillIconDisplay != null)
            skillIconDisplay.sprite = null;

        if (quantityText != null)
            quantityText.text = string.Empty;
    }
}
