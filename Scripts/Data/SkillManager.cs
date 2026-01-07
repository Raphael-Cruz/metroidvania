using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;


public class SkillManager : MonoBehaviour
{

    [Header("UI References")]
    public Transform gridParent;
    public Image SkillDisplayImage; 

    [Header("Data")]
    public List<SkillData> allSkills = new List<SkillData>();

private void OnEnable()
{
    RefreshGrid();

    // Auto-select the first collected skill so the screen isn't empty
    SkillSlot[] allSlots = gridParent.GetComponentsInChildren<SkillSlot>();
    foreach (var slot in allSlots)
    {
        if (slot.currentSkill != null && slot.currentSkill.isCollected)
        {
            slot.OnSelectSkill(); // Show this skill immediately
            break; 
        }
    }
}

    public void RefreshGrid()
    {
        if (gridParent == null) return;

        SkillSlot[] allSlots = gridParent.GetComponentsInChildren<SkillSlot>(true);

        for (int i = 0; i < allSlots.Length; i++)
        {
            // Pass the UI references to every slot
           
            allSlots[i].conceptArtDisplay = SkillDisplayImage;

            if (i < allSkills.Count && allSkills[i] != null)
            {
                allSlots[i].SetupSlot(allSkills[i]);
            }
            else
            {
                allSlots[i].SetAsEmpty();
            }
        }
    }
}