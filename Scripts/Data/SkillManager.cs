using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform gridParent;
    public Image skillDisplayImage;

    [Header("Skill Catalog (Assign Once)")]
    public List<SkillData> allSkills = new List<SkillData>();

    private void OnEnable()
    {
        RebuildFromWorldState();
        RefreshGrid();
        AutoSelectFirstCollectedSkill();
    }

    // 🔹 Pulls unlock info from WorldState
    void RebuildFromWorldState()
    {
        foreach (SkillData skill in allSkills)
        {
            skill.ResetRuntimeData();

            if (WorldState.UnlockedSkills.Contains(skill.skillID))
            {
                skill.isCollected = true;
                skill.currentQuantity = skill.maxQuantity;
            }
        }
    }

    public void RefreshGrid()
    {
        SkillSlot[] slots = gridParent.GetComponentsInChildren<SkillSlot>(true);

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].conceptArtDisplay = skillDisplayImage;

            if (i < allSkills.Count && allSkills[i] != null)
            {
                slots[i].SetupSlot(allSkills[i]);
            }
            else
            {
                slots[i].SetAsEmpty();
            }
        }
    }

    void AutoSelectFirstCollectedSkill()
    {
        if (skillDisplayImage != null)
        {
            skillDisplayImage.sprite = null;
            skillDisplayImage.color = new Color(0, 0, 0, 0.6f);
        }

        SkillSlot[] slots = gridParent.GetComponentsInChildren<SkillSlot>(true);

        foreach (var slot in slots)
        {
            if (slot.currentSkill != null && slot.currentSkill.isCollected)
            {
                slot.OnSelectSkill();
                break;
            }
        }
    }
}
